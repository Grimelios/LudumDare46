using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using GlmSharp;
using static Engine.Graphics.GL;
using static Engine.GLFW;

namespace Engine.Shaders
{
	public class Shader : IDisposable
	{
		private static readonly uint[] IntegerTypes =
		{
			GL_BYTE,
			GL_SHORT,
			GL_INT,
			GL_UNSIGNED_BYTE,
			GL_UNSIGNED_SHORT,
			GL_UNSIGNED_INT
		};

		private uint program;
		private uint vao;
		private uint vertexShader;
		private uint tesselationControlShader;
		private uint tesselationEvaluationShader;
		private uint geometryShader;
		private uint fragmentShader;
		private uint bufferId;
		private uint indexId;

		private bool isDisposed;

		private List<ShaderAttribute> attributes;
		private Dictionary<string, int> uniforms;

		public Shader() : this(0, 0)
		{
		}

		public Shader(uint bufferId) : this(bufferId, 0)
		{
		}

		public Shader(uint bufferId, uint indexId)
		{
			this.bufferId = bufferId;
			this.indexId = indexId;

			attributes = new List<ShaderAttribute>();
			uniforms = new Dictionary<string, int>();
		}

		public bool IsBindingComplete { get; private set; }

		public uint Stride { get; private set; }

		public void Attach(ShaderTypes shaderType, string filename)
		{
			uint id = Load(filename, (uint)shaderType);

			switch (shaderType)
			{
				case ShaderTypes.Vertex: vertexShader = id; break;
				case ShaderTypes.TessellationControl: tesselationControlShader = id; break;
				case ShaderTypes.TessellationEvaluation: tesselationEvaluationShader = id; break;
				case ShaderTypes.Geometry: geometryShader = id; break;
				case ShaderTypes.Fragment: fragmentShader = id; break;
			}
		}
	
		private unsafe uint Load(string filename, uint shaderType)
		{
			var id = glCreateShader(shaderType);
			var source = File.ReadAllText(Paths.Shaders + filename);
			var length = source.Length;

			glShaderSource(id, 1, new [] { source }, &length);
			glCompileShader(id);

			int status;
			
			glGetShaderiv(id, GL_COMPILE_STATUS, &status);

			if (status == GL_FALSE)
			{
				int logSize;

				glGetShaderiv(id, GL_INFO_LOG_LENGTH, &logSize);

				var message = new byte[logSize];

				fixed (byte* messagePointer = &message[0])
				{
					glGetShaderInfoLog(id, (uint)logSize, null, messagePointer);
				}

				glDeleteShader(id);

				Debug.Fail("Shader compilation failed: " + Encoding.Default.GetString(message));
			}

			return id;
		}

		public unsafe void Initialize()
		{
			Debug.Assert(vertexShader > 0 && fragmentShader > 0, "Can't initialize a shader without at least vertex " +
				"and fragment shaders attached.");

			program = glCreateProgram();
			glAttachShader(program, vertexShader);
			glAttachShader(program, fragmentShader);

			if (geometryShader != 0)
			{
				glAttachShader(program, geometryShader);
			}

			glLinkProgram(program);

			int status;

			glGetProgramiv(program, GL_LINK_STATUS, &status);

			if (status == GL_FALSE)
			{
				int logSize;

				glGetProgramiv(program, GL_INFO_LOG_LENGTH, &logSize);

				var message = new byte[logSize];

				fixed (byte* messagePointer = &message[0])
				{
					glGetProgramInfoLog(program, (uint)logSize, null, messagePointer);
				}

				glDeleteProgram(program);
				DeleteShaders();

				// See https://stackoverflow.com/a/11654597/7281613.
				Debug.Fail("Shader linking failed: " + Encoding.Default.GetString(message));
			}

			GetUniforms();
			DeleteShaders();

			if (bufferId > 0)
			{
				GenerateVao();
			}
		}

		private unsafe void GetUniforms()
		{
			int uniformCount;

			glGetProgramiv(program, GL_ACTIVE_UNIFORMS, &uniformCount);

			var bytes = new byte[64];

			fixed (byte* address = &bytes[0])
			{
				for (int i = 0; i < uniformCount; i++)
				{
					uint length;
					uint type;
					int size;

					glGetActiveUniform(program, (uint)i, (uint)bytes.Length, &length, &size, &type, address);

					var location = glGetUniformLocation(program, address);
					var name = Encoding.Default.GetString(bytes).Substring(0, (int)length);

					uniforms.Add(name, location);
				}
			}
		}

		private void DeleteShaders()
		{
			glDeleteShader(fragmentShader);
			glDeleteShader(tesselationControlShader);
			glDeleteShader(tesselationEvaluationShader);
			glDeleteShader(geometryShader);
			glDeleteShader(vertexShader);
		}

		public void AddAttribute<T>(uint count, uint type, bool isNormalized = false, uint padding = 0)
		{
			attributes.Add(new ShaderAttribute(count, type, Stride, isNormalized));

			// Padding is given in bytes directly (so that the padding can encompass data of multiple types).
			Stride += (uint)Marshal.SizeOf<T>() * count + padding;
		}

		public void Bind(uint bufferId, uint indexId)
		{
			Debug.Assert(!IsBindingComplete, "Shader was already bound.");

			this.bufferId = bufferId;
			this.indexId = indexId;

			GenerateVao();
		}

		private unsafe void GenerateVao()
		{
			fixed (uint* address = &vao)
			{
				glGenVertexArrays(1, address);
			}

			glBindVertexArray(vao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);

			if (indexId > 0)
			{
				glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
			}

			for (int i = 0; i < attributes.Count; i++)
			{
				var attribute = attributes[i];
				var index = (uint)i;
				var type = attribute.Type;

				// See https://community.khronos.org/t/vertex-shader-integer-input-broken/72878/2.
				/*
				if (IntegerTypes.Contains(type))
				{
					glVertexAttribIPointer(index, (int)attribute.Count, type, Stride, (void*)attribute.Offset);
				}
				else
				{
					glVertexAttribPointer(index, (int)attribute.Count, type, attribute.IsNormalized, Stride,
						(void*)attribute.Offset);
				}
				*/

				glVertexAttribPointer(index, (int)attribute.Count, type, attribute.IsNormalized, Stride,
					(void*)attribute.Offset);
				glEnableVertexAttribArray(index);
			}

			// This assumes that shaders won't be bound twice.
			attributes = null;
			IsBindingComplete = true;
		}

		public unsafe void Dispose()
		{
			Debug.Assert(!isDisposed, "Shader was already disposed.");

			glDeleteProgram(program);

			fixed (uint* address = &vao)
			{
				glDeleteVertexArrays(1, address);
			}

			isDisposed = true;
		}

		public void Use()
		{
			glUseProgram(program);
		}

		public void Apply()
		{
			if (vao == 0)
			{
				Debug.Fail("The shader's VAO was zero when applied. This likely means the shader was never bound.");
			}

			glUseProgram(program);
			glBindVertexArray(vao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
		}

		public bool HasUniform(string name)
		{
			return uniforms.ContainsKey(name);
		}

		public void SetUniform(string name, int value)
		{
			glUniform1i(uniforms[name], value);
		}

		public unsafe void SetUniform(string name, int[] value)
		{
			fixed (int* address = &value[0])
			{
				glUniform1iv(uniforms[name + "[0]"], (uint)value.Length, address);
			}
		}

		public void SetUniform(string name, float value)
		{
			glUniform1f(uniforms[name], value);
		}

		public unsafe void SetUniform(string name, float[] values)
		{
			fixed (float* address = &values[0])
			{
				glUniform1fv(uniforms[name + "[0]"], (uint)values.Length, address);
			}
		}

		public void SetUniform(string name, vec3 value)
		{
			glUniform3f(uniforms[name], value.x, value.y, value.z);
		}

		public void SetUniform(string name, vec4 value)
		{
			glUniform4f(uniforms[name], value.r, value.g, value.b, value.a);
		}

		public unsafe void SetUniform(string name, vec3[] values)
		{
			fixed (float* address = &values[0].x)
			{
				glUniform3fv(uniforms[name + "[0]"], (uint)values.Length, address);
			}
		}

		public unsafe void SetUniform(string name, vec4[] values)
		{
			fixed (float* address = &values[0].x)
			{
				glUniform4fv(uniforms[name + "[0]"], (uint)values.Length, address);
			}
		}

		public unsafe void SetUniform(string name, mat4 value)
		{
			var values = value.Values1D;

			fixed (float* address = &values[0])
			{
				glUniformMatrix4fv(uniforms[name], 1, false, address);
			}
		}

		public unsafe void SetUniform(string name, mat4[] values)
		{
			var floats = new float[values.Length * 16];

			for (int i = 0; i < values.Length; i++)
			{
				var start = i * 16;
				var array = values[i].Values1D;

				for (int j = 0; j < 16; j++)
				{
					floats[start + j] = array[j];
				}
			}

			fixed (float* address = &floats[0])
			{
				glUniformMatrix4fv(uniforms[name + "[0]"], (uint)values.Length, false, address);
			}
		}
	}
}

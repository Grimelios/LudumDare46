using System.Diagnostics;
using Engine.Core;
using Engine.Core._2D;
using Engine.Shaders;
using Engine.Shapes._2D;
using Engine.Utility;
using Engine.View._2D;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics._2D
{
	public class SpriteBatch
	{
		private Shader spriteShader;
		private Shader primitiveShader;
		private Shader activeShader;
		private PrimitiveBuffer buffer;

		private mat4 mvp;

		private uint mode;
		private uint activeTexture;

		public SpriteBatch(uint bufferCapacity = 0, uint indexCapacity = 0)
		{
			buffer = new PrimitiveBuffer(BufferFrequencies.Dynamic, BufferUsages.Draw, out var bufferId,
				out var indexId, bufferCapacity, indexCapacity);

			// These two shaders (owned by the sprite batch) can be completed here (in terms of binding a buffer).
			// External shaders are bound when first applied.
			spriteShader = new Shader(bufferId, indexId);
			spriteShader.Attach(ShaderTypes.Vertex, "Sprite.vert");
			spriteShader.Attach(ShaderTypes.Fragment, "Sprite.frag");
			spriteShader.AddAttribute<float>(2, GL_FLOAT);
			spriteShader.AddAttribute<float>(2, GL_FLOAT);
			spriteShader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);
			spriteShader.Initialize();

			primitiveShader = new Shader(bufferId, indexId);
			primitiveShader.Attach(ShaderTypes.Vertex, "Primitives2D.vert");
			primitiveShader.Attach(ShaderTypes.Fragment, "Primitives.frag");
			primitiveShader.AddAttribute<float>(2, GL_FLOAT);
			primitiveShader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);
			primitiveShader.Initialize();
		}

		public Camera2D Camera { get; set; }

		public uint Mode
		{
			get => mode;
			set
			{
				if (mode != value)
				{
					Flush();

					mode = value;
					buffer.Mode = value;
				}
			}
		}

		public void Dispose()
		{
			spriteShader.Dispose();
			primitiveShader.Dispose();
			buffer.Dispose();
		}

		public void Buffer(float[] data, ushort[] indices = null, int start = 0, int length = -1)
		{
			if (activeShader == null)
			{
				activeShader = spriteShader;
			}

			if (indices == null)
			{
				indices = new ushort[data.Length / (activeShader.Stride / sizeof(float))];

				for (int i = 0; i < indices.Length; i++)
				{
					indices[i] = (ushort)i;
				}
			}
			
			buffer.Buffer(data, indices, start, length);
		}

		public void Apply(Shader shader, uint mode)
		{
			Debug.Assert(shader != null, "Can't apply a null shader to the sprite batch.");
			Debug.Assert(shader.HasUniform("mvp"), "2D shaders must include a uniform MVP matrix.");

			if (activeShader == shader && this.mode == mode)
			{
				return;
			}

			Flush();
			activeShader = shader;
			Mode = mode;

			if (!activeShader.IsBindingComplete)
			{
				activeShader.Bind(buffer.BufferId, buffer.IndexId);
			}
		}

		public void ApplyTarget(RenderTarget renderTarget)
		{
			var halfDimensions = (renderTarget?.Dimensions ?? Resolution.WindowDimensions) / 2;

			mvp = mat4.Scale(1f / halfDimensions.x, 1f / halfDimensions.y, 1);
			mvp *= mat4.Translate(-halfDimensions.x, -halfDimensions.y, 0);

			renderTarget?.Apply();
		}

		public void BindTexture(uint id)
		{
			if (activeTexture == id)
			{
				return;
			}

			Flush();
			activeTexture = id;
		}

		public void Draw(Line2D line, Color color)
		{
			DrawLine(line.P1, line.P2, color);
		}

		public void Draw(Circle circle, int segments, Color color)
		{
			Debug.Assert(segments >= 3, "Can't render a 2D circle with fewer than three segments.");

			Apply(primitiveShader, GL_LINE_LOOP);

			var increment = Constants.TwoPi / segments;
			var data = new float[segments * 3];
			var f = color.ToFloat();

			for (int i = 0; i < segments; i++)
			{
				var p = circle.Position + Utilities.Direction(increment * i + circle.Rotation) * circle.Radius;
				var start = i * 3;

				data[start] = p.x;
				data[start + 1] = p.y;
				data[start + 2] = f;
			}

			Buffer(data);
		}

		public void Draw(Bounds2D bounds, Color color)
		{
			Draw(bounds.ToRectangle(), color);
		}

		public void Draw(Rectangle rect, Color color)
		{
			DrawInternal(rect, color, GL_LINE_LOOP, null);
		}

		public void DrawLine(vec2 p1, vec2 p2, Color color)
		{
			Apply(primitiveShader, GL_LINES);

			var f = color.ToFloat();
			var data = new []
			{
				p1.x,
				p1.y,
				f,
				p2.x,
				p2.y,
				f
			};

			ushort[] indices = { 0, 1 };

			Buffer(data, indices);
		}

		public void Fill(Bounds2D bounds, Color color)
		{
			Fill(bounds.ToRectangle(), color);
		}

		public void Fill(Rectangle rect, Color color)
		{
			ushort[] indices = { 0, 1, 3, 2 };

			DrawInternal(rect, color, GL_TRIANGLE_STRIP, indices);
		}

		private void DrawInternal(Rectangle rect, Color color, uint mode, ushort[] indices)
		{
			Apply(primitiveShader, mode);
			
			var corners = rect.Corners;
			var f = color.ToFloat();
			var data = new float[12];

			for (int i = 0; i < 4; i++)
			{
				var p = corners[i];
				var start = i * 3;

				data[start] = p.x;
				data[start + 1] = p.y;
				data[start + 2] = f;
			}

			Buffer(data, indices);
		}

		public unsafe void Flush()
		{
			if (buffer.BufferSize == 0)
			{
				return;
			}

			var result = mvp;

			if (Camera != null)
			{
				result *= Camera.MatrixInverse;
			}

			activeShader.Apply();
			activeShader.SetUniform("mvp", result);

			if (activeTexture != 0)
			{
				glActiveTexture(GL_TEXTURE0);
				glBindTexture(GL_TEXTURE_2D, activeTexture);
			}

			glDrawElements(mode, buffer.Flush(), GL_UNSIGNED_SHORT, null);

			activeShader = null;
			activeTexture = 0;
		}
	}
}

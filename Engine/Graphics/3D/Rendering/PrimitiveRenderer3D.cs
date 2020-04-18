using System;
using System.Diagnostics;
using Engine.Core;
using Engine.Shaders;
using Engine.Shapes._2D;
using Engine.Shapes._3D;
using Engine.Utility;
using Engine.View._3D;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class PrimitiveRenderer3D : IDisposable
	{
		private Camera3D camera;
		private Shader defaultShader;
		private Shader customShader;
		private PrimitiveBuffer buffer;

		private uint bufferId;
		private uint indexId;
		private uint mode;

		public PrimitiveRenderer3D(Camera3D camera = null, uint bufferCapacity = 0, uint indexCapacity = 0)
		{
			this.camera = camera;

			buffer = new PrimitiveBuffer(BufferFrequencies.Static, BufferUsages.Draw, out bufferId, out indexId,
				bufferCapacity, indexCapacity);

			defaultShader = new Shader(bufferId, indexId);
			defaultShader.Attach(ShaderTypes.Vertex, "Primitives3D.vert");
			defaultShader.Attach(ShaderTypes.Fragment, "Primitives.frag");
			defaultShader.AddAttribute<float>(3, GL_FLOAT);
			defaultShader.AddAttribute<byte>(4, GL_UNSIGNED_BYTE, true);
			defaultShader.Initialize();
		}

		public Camera3D Camera
		{
			set
			{
				Debug.Assert(value != null, "Can't nullify the camera for a 3D primitive renderer.");

				if (value != camera)
				{
					Flush();
					camera = value;
				}
			}
		}

		public void Apply(Shader shader)
		{
			Debug.Assert(shader != null, "Can't apply a null shader.");

			if (shader == customShader)
			{
				return;
			}

			Debug.Assert(shader.HasUniform("mvp"), "Custom shaders for 3D primitives must use a uniform MVP matrix.");

			Flush();
			customShader = shader;

			if (customShader != null && !customShader.IsBindingComplete)
			{
				customShader.Bind(bufferId, indexId);
			}
		}

		public void Dispose()
		{
			defaultShader.Dispose();
			buffer.Dispose();
		}

		public void Draw(Arc arc, float y, Color color, int segments = Constants.DefaultSegments)
		{
			Debug.Assert(segments > 0, "Primitive 3D arcs must be rendered using at least one segment.");

			var center = arc.Position;
			var points = new vec3[segments + 2];
			var start = arc.Angle - arc.Spread / 2;
			var increment = arc.Spread / segments;

			for (int i = 0; i <= segments; i++)
			{
				var p = center + Utilities.Direction(start + increment * i) * arc.Radius;

				points[i] = new vec3(p.x, y, p.y);
			}

			points[points.Length - 1] = new vec3(center.x, y, center.y);

			Buffer(points, color, GL_LINE_LOOP);
		}

		public void Draw(Box box, Color color)
		{
			var halfSize = new vec3(box.Width, box.Height, box.Depth) / 2;
			var min = -halfSize;
			var max = halfSize;
			var points = new []
			{
				min,
				new vec3(min.x, min.y, max.z),
				new vec3(min.x, max.y, max.z),
				new vec3(min.x, max.y, min.z),
				max,
				new vec3(max.x, max.y, min.z),
				new vec3(max.x, min.y, min.z),
				new vec3(max.x, min.y, max.z) 
			};

			if (box.IsOrientable)
			{
				for (int i = 0; i < points.Length; i++)
				{
					points[i] = box.Orientation * points[i];
				}
			}

			for (int i = 0; i < points.Length; i++)
			{
				points[i] += box.Position;
			}

			ushort[] indices =
			{
				0, 1, 1, 2, 2, 3, 3, 0,
				4, 5, 5, 6, 6, 7, 7, 4,
				0, 6, 1, 7, 2, 4, 3, 5
			};

			Buffer(points, color, GL_LINES, indices);
		}

		public void Draw(Capsule3D capsule, Color color, int segments = Constants.DefaultSegments, int rings = 5)
		{
			Debug.Assert(segments >= 3, "Primitive 3D capsules must be rendered using at least three segments (for " +
				"the circular portions).");
			Debug.Assert(rings > 0, "Primitive 3D capsules must be rendered using at least one ring (for the " +
				"hemisphere end caps).");

			var h = capsule.Height / 2;
			var r = capsule.Radius;
			var orientation = capsule.Orientation;
			var p = capsule.Position;
			var v = orientation * vec3.UnitY;

			// TODO: Draw lines from the top-most ring to the top endpoint (mirrored on the bottom too).
			for (int i = 0; i < rings; i++)
			{
				var radius = (float)Math.Cos(Constants.PiOverTwo / rings * i) * r;
				var offset = r / rings * i;

				DrawCircle(radius, p + v * (h + offset), orientation, color, segments);
				DrawCircle(radius, p - v * (h + offset), orientation, color, segments);
			}

			for (int i = 0; i < segments; i++)
			{
				var d = Utilities.Direction(Constants.TwoPi / segments * i) * r;
				var point = orientation * new vec3(d.x, 0, d.y) + p;

				DrawLine(point + v * h, point - v * h, color);
			}
		}

		public void Draw(Cylinder cylinder, Color color, int segments = Constants.DefaultSegments)
		{
			Debug.Assert(segments >= 3, "Cylinders must be renderer using at least three circular segments.");

			var increment = Constants.TwoPi / segments;
			var orientation = cylinder.Orientation;
			var center = cylinder.Position;
			var v = orientation * new vec3(0, cylinder.Height, 0);
			var c = center - v / 2;
			var radius = cylinder.Radius;

			DrawCircle(radius, c, orientation, color, segments);
			DrawCircle(radius, center + v / 2, orientation, color, segments);

			var points = new vec3[segments];

			for (int i = 0; i < points.Length; i++)
			{
				var p = Utilities.Direction(increment * i) * radius;

				points[i] = orientation * new vec3(p.x, 0, p.y) + c;
			}

			for (int i = 0; i < segments; i++)
			{
				var p = points[i];

				DrawLine(p, p + v, color);
			}
		}

		public void Draw(Sphere sphere, Color color, int segments = Constants.DefaultSegments)
		{
			Debug.Assert(segments >= 3, "Spheres must be renderer using at least three circular segments.");

			var radius = sphere.Radius;
			var center = sphere.Position;
			var orientation = sphere.Orientation;
			var q1 = orientation * quat.FromAxisAngle(Constants.PiOverTwo, vec3.UnitX);
			var q2 = orientation * quat.FromAxisAngle(Constants.PiOverTwo, vec3.UnitZ);

			DrawCircle(radius, center, orientation, color, segments);
			DrawCircle(radius, center, q1, color, segments);
			DrawCircle(radius, center, q2, color, segments);
		}

		public void DrawCircle(float radius, vec3 center, quat orientation, Color color,
			int segments = Constants.DefaultSegments)
		{
			Debug.Assert(segments >= 3, "Cylinders must be renderer using at least three circular segments.");

			var points = new vec3[segments];
			var increment = Constants.TwoPi / segments;

			for (int i = 0; i < points.Length; i++)
			{
				var p = Utilities.Direction(increment * i) * radius;

				points[i] = orientation * new vec3(p.x, 0, p.y) + center;
			}

			Buffer(points, color, GL_LINE_LOOP);
		}

		public void Draw(vec3 p, Color color)
		{
			Buffer(new [] { p }, color, GL_POINTS);
		}

		public void Draw(vec3[] triangle, Color color)
		{
			Buffer(triangle, color, GL_LINE_LOOP, new ushort[] { 0, 1, 2 });
		}

		public void Draw(Line3D line, Color color)
		{
			DrawLine(line.P1, line.P2, color);
		}

		public void DrawLine(vec3 p1, vec3 p2, Color color)
		{
			var points = new [] { p1, p2 };

			Buffer(points, color, GL_LINES);
		}

		public void DrawTriangle(vec3 p0, vec3 p1, vec3 p2, Color color)
		{
			Draw(new [] { p0, p1, p2 }, color);
		}

		public void Fill(vec3[] strip, Color color)
		{
			Buffer(strip, color, GL_TRIANGLE_STRIP);
		}

		private float[] GetData(vec3[] points, Color color)
		{
			var f = color.ToFloat();
			var data = new float[points.Length * 4];

			for (int i = 0; i < points.Length; i++)
			{
				var start = i * 4;
				var p = points[i];

				data[start] = p.x;
				data[start + 1] = p.y;
				data[start + 2] = p.z;
				data[start + 3] = f;
			}

			return data;
		}

		private void Buffer(vec3[] points, Color color, uint mode, ushort[] indices = null)
		{
			VerifyMode(mode);

			var data = GetData(points, color);

			// If the index array is null, it's assumed that indexes can be added sequentially.
			if (indices == null)
			{
				indices = new ushort[points.Length];

				for (int i = 0; i < indices.Length; i++)
				{
					indices[i] = (ushort)i;
				}
			}

			buffer.Buffer(data, indices);
		}

		private void VerifyMode(uint mode)
		{
			if (this.mode != mode)
			{
				Flush();

				this.mode = mode;

				buffer.Mode = mode;
			}
		}

		public void Clear()
		{
			buffer.Clear();
		}

		public unsafe void Flush()
		{
			if (buffer.BufferSize == 0)
			{
				return;
			}

			Shader shader;

			// If a custom shader is used, it's assumed to only apply to the current batch of primitives.
			if (customShader != null)
			{
				shader = customShader;
				customShader = null;
			}
			else
			{
				shader = defaultShader;
			}

			shader.Apply();
			shader.SetUniform("mvp", camera.ViewProjection);

			glDrawElements(mode, buffer.Flush(), GL_UNSIGNED_SHORT, null);
		}
	}
}

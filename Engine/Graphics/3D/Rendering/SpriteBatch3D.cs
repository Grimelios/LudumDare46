using System.Collections.Generic;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Lighting;
using Engine.Shaders;
using Engine.View._3D;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class SpriteBatch3D : AbstractRenderer3D<(uint SourceId, SourceRect SourceRect), Sprite3D>
	{
		private List<float> buffer;

		private bool needsUpload;

		public unsafe SpriteBatch3D(Camera3D camera, GlobalLight3D light) : base(camera, light)
		{
			GLUtilities.GenerateBuffers(out bufferId, out indexId);

			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Sprite3D.vert");
			shader.Attach(ShaderTypes.Fragment, "Sprite3D.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);

			Bind(bufferId, indexId);
			buffer = new List<float>();

			// Buffer index data (shared among all sprites).
			var indices = new ushort[4];

			for (int i = 0; i < indices.Length; i++)
			{
				indices[i] = (ushort)i;
			}

			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);

			fixed (ushort* address = &indices[0])
			{
				glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(ushort) * 4, address, GL_STATIC_DRAW);
			}
		}

		public override void Add(Sprite3D sprite)
		{
			var source = sprite.Source;
			var rect = sprite.SourceRect;

			Add((source.Id, rect), sprite);

			var points = new []
			{
				-vec2.Ones,
				new vec2(-1, 1),
				new vec2(1, -1),
				vec2.Ones
			};

			// This gives the square a unit length of one.
			for (int i = 0; i < points.Length; i++)
			{
				points[i] /= 2;
			}

			var w = (float)source.Width;
			var h = (float)source.Height;

			vec2[] sourceCoords;

			if (rect.Width > 0)
			{
				sourceCoords = new []
				{
					new vec2(rect.Left / w, rect.Bottom / h),
					new vec2(rect.Left / w, rect.Top / h),
					new vec2(rect.Right / w, rect.Bottom / h),
					new vec2(rect.Right / w, rect.Top / h)
				};
			}
			else
			{
				sourceCoords = new []
				{
					vec2.Zero,
					vec2.UnitY,
					vec2.UnitX,
					vec2.Ones
				};
			}

			var correction = new vec2(0.5f / w, 0.5f / h);

			for (int i = 0; i < 4; i++)
			{
				sourceCoords[i] += correction;
			}

			var data = new float[20];

			for (int i = 0; i < 4; i++)
			{
				var p = points[i];
				var s = sourceCoords[i];
				var start = i * 5;

				data[start] = p.x;
				data[start + 1] = p.y;
				data[start + 2] = 0;
				data[start + 3] = s.x;
				data[start + 4] = s.y;
			}

			sprite.BaseVertex = (buffer.Count / 20) * 4;
			buffer.AddRange(data);
			needsUpload = true;
		}

		public override void Remove(Sprite3D sprite)
		{
			Remove((sprite.Source.Id, sprite.SourceRect), sprite);
		}

		public void Remove(IEnumerable<Sprite3D> sprites)
		{
			foreach (var s in sprites)
			{
				Remove(s);
			}
		}

		public override unsafe void PrepareShadow()
		{
			if (needsUpload)
			{
				var data = buffer.ToArray();

				glBindBuffer(GL_ARRAY_BUFFER, bufferId);

				fixed (float* address = &data[0])
				{
					glBufferData(GL_ARRAY_BUFFER, sizeof(float) * (uint)buffer.Count, address, GL_STATIC_DRAW);
				}

				needsUpload = false;
			}

			glDisable(GL_CULL_FACE);

			base.PrepareShadow();
		}

		protected override void Apply((uint SourceId, SourceRect SourceRect) key)
		{
			// For 3D sprites, the key is the source ID.
			glActiveTexture(GL_TEXTURE0);
			glBindTexture(GL_TEXTURE_2D, key.SourceId);
		}

		public override unsafe void Draw(Sprite3D item, mat4? vp)
		{
			if (vp != null)
			{
				PrepareShader(item, vp.Value);
			}

			shader.SetUniform("color", item.Color.ResultValue.ToVec4());
			glDrawElementsBaseVertex(GL_TRIANGLE_STRIP, 4, GL_UNSIGNED_SHORT, (void*)0, item.BaseVertex);
		}
	}
}

using System.Diagnostics;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Shaders;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Core._2D
{
	public abstract class Component2D : IRenderable2D, IRenderScalable2D, IRenderColorable
	{
		// Each vertex contains position, texture coordinates, and color.
		protected const int VertexSize = 5;
		protected const int QuadSize = VertexSize * 4;

		private RenderPosition2DField positionField;
		private RenderScale2DField scaleField;
		private RenderRotationField rotationField;
		private RenderColorField colorField;

		private bool isDisposed;

		protected ivec2 origin;

		protected float[] data;

		protected Flags<TransformFlags> flags;

		protected Component2D(Alignments alignment, SpriteModifiers mods)
		{
			Alignment = alignment;
			Mods = mods;
			flags = new Flags<TransformFlags>(TransformFlags.IsSourceChanged);
			positionField = new RenderPosition2DField(flags);
			scaleField = new RenderScale2DField(flags);
			rotationField = new RenderRotationField(flags);
			colorField = new RenderColorField(flags);
			IsDrawEnabled = true;
		}

		protected Alignments Alignment { get; }

		public bool IsDrawEnabled { get; set; }

		public RenderPosition2DField Position => positionField;
		public RenderScale2DField Scale => scaleField;
		public RenderColorField Color => colorField;
		public RenderRotationField Rotation => rotationField;

		// Using a custom shader is optional.
		public Shader Shader { get; set; }

		public SpriteModifiers Mods { get; set; }

		protected abstract void RecomputeTransformData(vec2 p, vec2 scale, float rotation);
		protected abstract void RecomputeSourceData();
		
		public void Dispose()
		{
			Debug.Assert(!isDisposed, "Component was already disposed.");

			Shader?.Dispose();
			isDisposed = true;
		}

		public void Recompute(float t)
		{
			var v = flags.Value;

			// Source is intentionally computed before transform to make sure the proper origin is applied.
			if ((v & TransformFlags.IsSourceChanged) > 0)
			{
				RecomputeSourceData();
				flags.Value = v & ~TransformFlags.IsSourceChanged;
			}

			var p = positionField.Evaluate(t);
			var s = scaleField.Evaluate(t);
			var r = rotationField.Evaluate(t);

			RecomputeTransformData(p, s, r);
			RecomputeColorData(t);
		}

		public abstract void Draw(SpriteBatch sb, float t);

		protected void Draw(SpriteBatch sb, float t, uint textureId, float[] data)
		{
			// Data can be null for empty text.
			if (data == null)
			{
				return;
			}

			Recompute(t);
			sb.BindTexture(textureId);

			if (Shader != null)
			{
				sb.Apply(Shader, GL_TRIANGLE_STRIP);
			}
			else
			{
				sb.Mode = GL_TRIANGLE_STRIP;
			}

			// Strings need to buffer each character individually in order to add the primitive restart index each
			// time.
			var quads = data.Length / QuadSize;
			var indices = new ushort[] { 0, 1, 2, 3 };

			for (int i = 0; i < quads; i++)
			{
				sb.Buffer(data, indices, i * QuadSize, QuadSize);
			}
		}

		private void RecomputeColorData(float t)
		{
			// Data can be null for empty text.
			if (data == null)
			{
				return;
			}

			var c = colorField.Evaluate(t);
			var f = c.ToFloat();

			for (int i = 0; i < data.Length / VertexSize; i++)
			{
				data[i * VertexSize + 4] = f;
			}
		}
	}
}

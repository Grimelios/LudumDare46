using Engine.Content;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Interfaces._3D;
using Engine.Shaders;
using GlmSharp;

namespace Engine.Core._3D
{
	public class Sprite3D : IRenderable3D, IRenderScalable2D, IRenderColorable
	{
		// This value scales pixels to in-game units. When unscaled, every X pixels spans one unit.
		public const int PixelDivisor = 100;

		private RenderPosition3DField positionField;
		private RenderScale2DField scaleField;
		private RenderOrientationField orientationField;
		private RenderColorField colorField;

		// This is conceptually similar to 2D sprite origins, but is instead used to offset position. Using floats
		// (i.e. vec2 rather than ivec2) gives more precision.
		private vec2 correction;

		public Sprite3D(string filename, Alignments alignment = Alignments.Center) :
			this(ContentCache.GetTexture(filename), new SourceRect(), alignment)
		{
		}

		public Sprite3D(string filename, SourceRect sourceRect, Alignments alignment = Alignments.Center) :
			this(ContentCache.GetTexture(filename), sourceRect, alignment)
		{
		}

		public Sprite3D(QuadSource source, Alignments alignment = Alignments.Center) :
			this(source, new SourceRect(), alignment)
		{
		}

		public Sprite3D(QuadSource source, SourceRect sourceRect, ivec2 origin) :
			this(source, sourceRect, Alignments.Custom)
		{
			int w;
			int h;

			if (sourceRect.Width > 0)
			{
				w = sourceRect.Width;
				h = sourceRect.Height;
			}
			else
			{
				w = source.Width;
				h = source.Height;
			}

			correction = origin - new vec2(w, h) / 2;
		}

		public Sprite3D(QuadSource source, SourceRect sourceRect, Alignments alignment = Alignments.Center)
		{
			Source = source;
			SourceRect = sourceRect;

			int w;
			int h;

			if (sourceRect.Width > 0)
			{
				w = sourceRect.Width;
				h = sourceRect.Height;
			}
			else
			{
				w = source.Width;
				h = source.Height;
			}
			
			if (alignment != Alignments.Custom)
			{
				bool left = (alignment & Alignments.Left) > 0;
				bool right = (alignment & Alignments.Right) > 0;
				bool top = (alignment & Alignments.Top) > 0;
				bool bottom = (alignment & Alignments.Bottom) > 0;

				var x = left ? 0 : (right ? w : w / 2f);
				var y = top ? 0 : (bottom ? h : h / 2f);

				correction = new vec2(x, y) - new vec2(w, h) / 2;
			}

			var flags = new Flags<TransformFlags>(TransformFlags.None);

			IsDrawEnabled = true;
			IsShadowCaster = true;
			positionField = new RenderPosition3DField(flags);
			scaleField = new RenderScale2DField(flags);
			scaleField.SetValue(new vec2(w, h) / PixelDivisor,  false);
			orientationField = new RenderOrientationField(flags);
			colorField = new RenderColorField(flags);
		}

		internal int BaseVertex { get; set; }

		internal QuadSource Source { get; }
		internal SourceRect SourceRect { get; }

		public RenderPosition3DField Position => positionField;
		public RenderScale2DField Scale => scaleField;
		public RenderOrientationField Orientation => orientationField;
		public RenderColorField Color => colorField;

		public mat4 WorldMatrix { get; private set; }

		// TODO: Support billboarding (requires camera access).
		public bool IsBillboarded { get; set; }
		public bool IsDrawEnabled { get; set; }
		public bool IsShadowCaster { get; set; }

		// Using custom shaders is optional.
		public Shader Shader { get; set; }
		public Shader ShadowShader { get; set; }

		public void Recompute(float t)
		{
			colorField.Evaluate(t);

			var p = positionField.Evaluate(t);
			var s = scaleField.Evaluate(t);
			var q = orientationField.Evaluate(t);

			// By shifting the world matrix using the origin, all sprites using the same source rect and alignment
			// (regardless of transform) can be rendered using the same quad in GPU memory. This is most applicable to
			// 3D sprites that don't use a source rect at all (since they can all use the same unit square).
			var v = new vec3(correction / PixelDivisor, 0);

			WorldMatrix = mat4.Translate(p) * q.ToMat4 * mat4.Translate(v) * mat4.Scale(new vec3(s, 0));
		}

		// TODO: Do textures need to be unloaded? (from the content cache and/or GPU memory)
		public void Dispose()
		{
			Shader?.Dispose();
			ShadowShader?.Dispose();
		}
	}
}

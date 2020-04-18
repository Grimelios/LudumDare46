using System.Diagnostics;
using System.Linq;
using Engine.Content;
using Engine.Graphics;
using Engine.Graphics._2D;
using Engine.Utility;
using GlmSharp;

namespace Engine.Core._2D
{
	public class Sprite : Component2D
	{
		private QuadSource source;
		private SourceRect sourceRect;

		public Sprite(string filename, Alignments alignment = Alignments.Center,
			SpriteModifiers mods = SpriteModifiers.None, TextureWraps wrap = TextureWraps.Repeat,
			TextureFilters filter = TextureFilters.Nearest) :
			this(ContentCache.GetTexture(filename, false, true, wrap, filter), new SourceRect(), alignment, mods)
		{
		}

		public Sprite(string filename, SourceRect sourceRect, Alignments alignment = Alignments.Center,
			SpriteModifiers mods = SpriteModifiers.None, TextureWraps wrap = TextureWraps.Repeat,
			TextureFilters filter = TextureFilters.Nearest) :
			this (ContentCache.GetTexture(filename, false, true, wrap, filter), sourceRect, alignment, mods)
		{
		}

		public Sprite(string filename, ivec2 origin, TextureWraps wrap = TextureWraps.Repeat,
			TextureFilters filter = TextureFilters.Nearest) :
			this(ContentCache.GetTexture(filename, false, true, wrap, filter), new SourceRect(),
			Alignments.Custom)
		{
			this.origin = origin;
		}

		public Sprite(QuadSource source, Alignments alignment = Alignments.Center,
			SpriteModifiers mods = SpriteModifiers.None) : this(source, new SourceRect(), alignment, mods)
		{
		}

		public Sprite(QuadSource source, SourceRect sourceRect, Alignments alignment = Alignments.Center,
			SpriteModifiers mods = SpriteModifiers.None) : base(alignment, mods)
		{
			this.source = source;
			this.sourceRect = sourceRect;

			data = new float[QuadSize];
		}

		public Sprite(QuadSource source, ivec2 origin) : this(source, new SourceRect(), Alignments.Custom)
		{
			this.origin = origin;
		}

		public SourceRect SourceRect
		{
			get => sourceRect;
			set
			{
				if (sourceRect.Width == 0 && Alignment == Alignments.Custom)
				{
					// TODO: Why not? Might want to remove this restriction (if needed). Can't remember why I added it.
					Debug.Fail("Can't set a source rectangle on sprites with custom origins.");
				}

				sourceRect = value;
				flags.Value |= TransformFlags.IsSourceChanged;
			}
		}

		public void ScaleTo(int width, int height, bool shouldInterpolate)
		{
			float w;
			float h;

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

			Scale.SetValue(new vec2(width / w, height / h), shouldInterpolate);
		}

		protected override void RecomputeTransformData(vec2 p, vec2 scale, float rotation)
		{
			float width;
			float height;

			if (sourceRect.Width > 0)
			{
				width = sourceRect.Width;
				height = sourceRect.Height;
			}
			else
			{
				width = source.Width;
				height = source.Height;
			}

			width *= scale.x;
			height *= scale.y;

			var points = new []
			{
				new vec2(0, 0),
				new vec2(0, height),
				new vec2(width, 0),
				new vec2(width, height)
			};

			RecomputeOrigin(scale);

			for (int i = 0; i < 4; i++)
			{
				var point = points[i];
				point -= origin;
				points[i] = point;
			}

			if (rotation != 0)
			{
				var matrix = Utilities.RotationMatrix2D(rotation);

				for (int i = 0; i < 4; i++)
				{
					var point = points[i];
					point = matrix * point;
					points[i] = point;
				}
			}

			var order = Enumerable.Range(0, 4).ToArray();
			var flipHorizontal = (Mods & SpriteModifiers.FlipHorizontal) > 0;
			var flipVertical = (Mods & SpriteModifiers.FlipVertical) > 0;

			if (flipHorizontal)
			{
				order[0] = 2;
				order[1] = 3;
				order[2] = 0;
				order[3] = 1;
			}

			if (flipVertical)
			{
				var temp1 = order[0];
				var temp2 = order[2];

				order[0] = order[1];
				order[1] = temp1;
				order[2] = order[3];
				order[3] = temp2;
			}

			for (int i = 0; i < 4; i++)
			{
				var start = i * VertexSize;
				var point = points[order[i]] + p;

				data[start] = point.x;
				data[start + 1] = point.y;
			}
		}

		private void RecomputeOrigin(vec2 scale)
		{
			int width;
			int height;

			if (sourceRect.Width > 0)
			{
				width = sourceRect.Width;
				height = sourceRect.Height;
			}
			else
			{
				width = source.Width;
				height = source.Height;
			}

			// TODO: Should origin not be cast to an integer? Might lose accuracy and smoothness.
			origin = Utilities.ComputeOrigin((int)(width * scale.x), (int)(height * scale.y), Alignment);
		}

		protected override void RecomputeSourceData()
		{
			var coords = new vec2[4];

			if (sourceRect.Width > 0)
			{
				coords[0] = new vec2(sourceRect.Left, sourceRect.Top);
				coords[1] = new vec2(sourceRect.Left, sourceRect.Bottom);
				coords[2] = new vec2(sourceRect.Right, sourceRect.Top);
				coords[3] = new vec2(sourceRect.Right, sourceRect.Bottom);

				var dimensions = new vec2(source.Width, source.Height);

				for (int i = 0; i < 4; i++)
				{
					coords[i] /= dimensions;
				}
			}
			else
			{
				coords[0] = vec2.Zero;
				coords[1] = vec2.UnitY;
				coords[2] = vec2.UnitX;
				coords[3] = vec2.Ones;
			}

			for (int i = 0; i < 4; i++)
			{
				var value = coords[i];
				var start = i * VertexSize + 2;

				data[start] = value.x;
				data[start + 1] = value.y;
			}
		}

		public override void Draw(SpriteBatch sb, float t)
		{
			if (!IsDrawEnabled)
			{
				return;
			}

			Draw(sb, t, source.Id, data);
		}	
	}
}

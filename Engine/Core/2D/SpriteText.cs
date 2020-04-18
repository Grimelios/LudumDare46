using System.Diagnostics;
using Engine.Content;
using Engine.Graphics._2D;
using Engine.Utility;
using GlmSharp;

namespace Engine.Core._2D
{
	[DebuggerDisplay("{value")]
	public class SpriteText : Component2D
	{
		private SpriteFont font;

		private string value;

		private bool useLiteralMeasuring;

		public SpriteText(string font, string value = null, Alignments alignment = Alignments.Left | Alignments.Top,
			bool useLiteralMeasuring = false) :
			this(ContentCache.GetFont(font), value, alignment)
		{
		}

		public SpriteText(SpriteFont font, string value = null, Alignments alignment = Alignments.Left |
			Alignments.Top, bool useLiteralMeasuring = false) : this(font, vec2.Zero, value, alignment)
		{
		}

		// TODO: Support modifiers on text (e.g. flip vertical or horizontal).
		public SpriteText(SpriteFont font, vec2 position, string value = null,
			Alignments alignment = Alignments.Left | Alignments.Top, bool useLiteralMeasuring = false) :
			base(alignment, SpriteModifiers.None)
		{
			this.font = font;
			this.useLiteralMeasuring = useLiteralMeasuring;

			Value = value;
			Position.SetValue(position, false);
		}

		public string Value
		{
			get => value;
			set
			{
				this.value = value;
				
				if (!string.IsNullOrEmpty(value))
				{
					var offset = ivec2.Zero;
					var dimensions = useLiteralMeasuring
						? font.MeasureLiteral(value, out offset)
						: font.Measure(value);

					origin = Utilities.ComputeOrigin(dimensions.x, dimensions.y, Alignment) - offset;

					// Each character from the string is rendered as a quad.
					data = new float[value.Length * QuadSize];
					flags.Value |= TransformFlags.IsSourceChanged;
				}
				else
				{
					data = null;
				}
			}
		}

		public int Size => font.Size;
		public int Length => value?.Length ?? 0;

		public bool UseLiteralMeasuring
		{
			get => useLiteralMeasuring;
			set
			{
				if (value != useLiteralMeasuring)
				{
					useLiteralMeasuring = value;

					// This causes position and origin to be recomputed.
					Value = this.value;
				}
			}
		}

		// TODO: Support rotation and scale.
		protected override void RecomputeTransformData(vec2 p, vec2 scale, float rotation)
		{
			if (value == null)
			{
				return;
			}

			var index = 0;
			var glyphs = font.Glyphs;

			// TODO: Should position be cast to integers here? (helps prevent fuzziness).
			var localPosition = p - origin;

			foreach (char c in value)
			{
				var glyph = glyphs[c];

				// Spaces advance the next character position, but aren't rendered.
				if (c != ' ')
				{
					var offset = glyph.Offset;
					var point = localPosition + offset;
					var left = point.x;
					var right = point.x + glyph.Width;
					var top = point.y;
					var bottom = point.y + glyph.Height;

					var array = new []
					{
						new vec2(left, top),
						new vec2(left, bottom),
						new vec2(right, top), 
						new vec2(right, bottom) 
					};

					for (int i = 0; i < 4; i++)
					{
						var a = array[i];

						data[index] = a.x;
						data[index + 1] = a.y;

						index += VertexSize;
					}
				}

				localPosition.x += glyph.Advance;
			}
		}

		protected override void RecomputeSourceData()
		{
			if (value == null)
			{
				return;
			}

			var index = 2;
			var glyphs = font.Glyphs;

			foreach (char c in value)
			{
				if (c == ' ')
				{
					continue;
				}

				var glyph = glyphs[c];

				foreach (vec2 p in glyph.Source)
				{
					data[index] = p.x;
					data[index + 1] = p.y;

					index += VertexSize;
				}
			}
		}

		public override void Draw(SpriteBatch sb, float t)
		{
			if (string.IsNullOrEmpty(value) || !IsDrawEnabled)
			{
				return;
			}

			Draw(sb, t, font.TextureId, data);
		}
	}
}

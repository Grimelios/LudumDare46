using System;
using System.IO;
using System.Linq;
using Engine.Content;
using GlmSharp;

namespace Engine.Graphics._2D
{
	public class SpriteFont
	{
		// This covers common English characters, numbers, and punctuation.
		private const int CharacterRange = 127;

		public static SpriteFont Load(string name, bool shouldCache = true)
		{
			var texture = ContentCache.GetTexture(name + "_0.png", false, shouldCache, TextureWraps.Repeat,
				TextureFilters.Nearest, "Fonts/");
			var lines = File.ReadAllLines(Paths.Fonts + name + ".fnt");
			var first = lines[0];

			// Size is in the form "size=[value]";
			var index1 = first.IndexOf("size", StringComparison.CurrentCulture) + 5;
			var index2 = first.IndexOf(' ', index1);
			var size = int.Parse(first.Substring(index1, index2 - index1));

			// Character count (the fourth line) looks like "chars count=[value]".
			var count = int.Parse(lines[3].Substring(12));
			var glyphs = new Glyph[CharacterRange];

			for (int i = 0; i < count; i++)
			{
				int ParseValue(string s)
				{
					var index = s.IndexOf('=') + 1;

					return int.Parse(s.Substring(index));
				}

				var line = lines[i + 4];
				var tokens = line.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);

				// The first token is just "char", so it can be ignored.
				var id = ParseValue(tokens[1]);
				var x = ParseValue(tokens[2]);
				var y = ParseValue(tokens[3]);
				var width = ParseValue(tokens[4]);
				var height = ParseValue(tokens[5]);
				var offsetX = ParseValue(tokens[6]);
				var offsetY = ParseValue(tokens[7]);
				var advance = ParseValue(tokens[8]);
				var w = texture.Width;
				var h = texture.Height;

				vec2[] source =
				{
					new vec2(x, y),
					new vec2(x, y + height),
					new vec2(x + width, y),
					new vec2(x + width, y + height) 
				};

				for (int j = 0; j < 4; j++)
				{
					source[j] /= new vec2(w, h);
				}

				glyphs[id] = new Glyph(width, height, advance, new ivec2(offsetX, offsetY), source);
			}

			return new SpriteFont(glyphs, size, texture.Id);
		}

		private SpriteFont(Glyph[] glyphs, int size, uint textureId)
		{
			Glyphs = glyphs;
			Size = size;
			TextureId = textureId;
		}

		public Glyph[] Glyphs { get; }

		public int Size { get; }

		public uint TextureId { get; }

		public ivec2 Measure(string value)
		{
			if (value.Length == 0)
			{
				return ivec2.Zero;
			}

			var sumWidth = value.Sum(c => Glyphs[c].Advance);

			return new ivec2(sumWidth, Size);
		}

		public ivec2 MeasureLiteral(string value, out ivec2 offset)
		{
			offset = ivec2.Zero;

			if (value.Length == 0)
			{
				return ivec2.Zero;
			}

			var sumWidth = 0;
			var top = int.MaxValue;
			var bottom = 0;
			var length = value.Length;

			for (int i = 0; i < length; i++)
			{
				var glyph = Glyphs[value[i]];
				var x = glyph.Offset.x;
				var y = glyph.Offset.y;
				var advance = glyph.Advance;

				if (i == 0)
				{
					offset.x = x;
					sumWidth += advance - x;
				}
				else if (i == length - 1)
				{
					sumWidth += x + glyph.Width;
				}
				else
				{
					sumWidth += advance;
				}

				top = Math.Min(top, y);
				bottom = Math.Max(bottom, y + glyph.Height);
			}

			offset.y = top;

			return new ivec2(sumWidth, bottom - top);
		}
	}
}

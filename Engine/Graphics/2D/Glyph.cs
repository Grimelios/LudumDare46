using GlmSharp;

namespace Engine.Graphics._2D
{
	public class Glyph
	{
		public Glyph(int width, int height, int advance, ivec2 offset, vec2[] source)
		{
			Width = width;
			Height = height;
			Advance = advance;
			Offset = offset;
			Source = source;
		}

		public int Width { get; }
		public int Height { get; }
		public int Advance { get; }

		public ivec2 Offset { get; }
		public vec2[] Source { get; }
	}
}

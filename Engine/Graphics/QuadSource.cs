using System;
using System.Diagnostics;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics
{
	[DebuggerDisplay("Id={Id}, Dimensions={Width}x{Height}")]
	public abstract class QuadSource : IDisposable
	{
		private int width;
		private int height;

		protected QuadSource() : this(0, 0, 0)
		{
		}

		protected QuadSource(int width, int height) : this(0, width, height)
		{
		}

		protected QuadSource(uint id, int width, int height)
		{
			Id = id;
			Width = width;
			Height = height;
		}

		public uint Id { get; protected set; }

		public int Width
		{
			get => width;
			set
			{
				Debug.Assert(value > 0, "Quad source width must be positive.");
				
				width = value;
			}
		}

		public int Height
		{
			get => height;
			set
			{
				Debug.Assert(value > 0, "Quad source height must be positive.");

				height = value;
			}
		}

		public ivec2 Dimensions => new ivec2(Width, Height);

		public void Bind(uint index)
		{
			glActiveTexture(GL_TEXTURE0 + index);
			glBindTexture(GL_TEXTURE_2D, Id);
		}

		public virtual unsafe void Dispose()
		{
			var id = Id;

			glDeleteTextures(1, &id);
		}
	}
}

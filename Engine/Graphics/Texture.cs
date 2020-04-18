using System.Diagnostics;
using System.Drawing;
using System.Linq;
using static Engine.Graphics.GL;

namespace Engine.Graphics
{
	public class Texture : QuadSource
	{
		public static unsafe Texture Load(string filename, string folder, bool shouldStoreData = false,
			TextureWraps wrap = TextureWraps.Repeat, TextureFilters filter = TextureFilters.Nearest)
		{
			LoadData(folder + filename, out var width, out var height, out var data);

			uint id = 0;

			glGenTextures(1, &id);

			// Setting data also binds the texture.
			var texture = new Texture(id, width, height);
			texture.SetData(data, shouldStoreData);

			SetParameters(wrap, filter);

			return texture;
		}

		// This function is useful externally to load texture data without using OpenGL calls. Useful when a texture
		// is used for purposes other than direct rendering.
		public static void LoadData(string path, out int width, out int height, out int[] data)
		{
			using var image = new Bitmap(Paths.Content + path);

			width = image.Width;
			height = image.Height;
			data = new int[width * height];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					data[i * width + j] = image.GetPixel(j, i).ToRgba();
				}
			}
		}

		private static void SetParameters(TextureWraps wrap, TextureFilters filter)
		{
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)filter);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)filter);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int)wrap);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int)wrap);
		}

		// Only some textures store their data in non-GPU memory (e.g. detecting hitscan collisions on Doom-like
		// enemies using their texture).
		private int[] data;

		// These public constructors is useful when generating textures dynamically at runtime.
		public Texture(int width, int height, Core.Color fill, TextureWraps wrap = TextureWraps.Repeat,
			TextureFilters filter = TextureFilters.Nearest) : this(width, height, wrap, filter)
		{
			SetData(width, height, Enumerable.Repeat(fill.ToRgba(), width * height).ToArray());
		}

		public Texture(int width, int height, Core.Color[] data, TextureWraps wrap = TextureWraps.Repeat,
			TextureFilters filter = TextureFilters.Nearest) : this(width, height, wrap, filter)
		{
			SetData(width, height, data.Select(color => color.ToRgba()).ToArray());
		}

		public unsafe Texture(int width, int height, TextureWraps wrap = TextureWraps.Repeat,
			TextureFilters filter = TextureFilters.Nearest) : base(width, height)
		{
			uint id = 0;

			glGenTextures(1, &id);
			SetParameters(wrap, filter);
			Id = id;
		}

		private Texture(uint id, int width, int height) : base(id, width, height)
		{
		}

		public int[] Data => data;

		public void SetData(int width, int height, int[] data, bool shouldStore = false)
		{
			Width = width;
			Height = height;

			SetData(data, shouldStore);
		}

		private unsafe void SetData(int[] data, bool shouldStore = false)
		{
			Debug.Assert(data.Length == Width * Height, $"Texture data array is the wrong size (given {data.Length} " +
				$"elements, but expected {Width}x{Height} = {Width * Height}).");

			if (shouldStore)
			{
				this.data = data;
			}

			glBindTexture(GL_TEXTURE_2D, Id);

			fixed (int* pointer = &data[0])
			{
				glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, (uint)Width, (uint)Height, 0, GL_RGBA, GL_UNSIGNED_BYTE,
					pointer);
			}
		}
	}
}

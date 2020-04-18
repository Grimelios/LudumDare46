using System;

namespace Engine.Graphics
{
	public class Window
	{
		public Window(int width, int height, IntPtr address)
		{
			Width = width;
			Height = height;
			Address = address;
		}

		public int Width { get; }
		public int Height { get; }

		public IntPtr Address { get; }
	}
}

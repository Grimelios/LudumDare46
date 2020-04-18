using System;
using Engine.Graphics._2D;
using Engine.Shaders;

namespace Engine.Interfaces._2D
{
	public interface IRenderable2D : IRenderTransformable2D, IDisposable
	{
		Shader Shader { get; set; }

		void Draw(SpriteBatch sb, float t);
	}
}

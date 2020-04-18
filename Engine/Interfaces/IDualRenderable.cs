using Engine.Graphics._2D;

namespace Engine.Interfaces
{
	public interface IDualRenderable
	{
		void Draw2D(SpriteBatch sb, float t);
		void Draw3D();
	}
}

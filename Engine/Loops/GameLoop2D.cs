using Engine.Graphics._2D;
using Engine.Interfaces._2D;
using Engine.View._2D;

namespace Engine.Loops
{
	public abstract class GameLoop2D<TType> : GameLoop<TType, Camera2D, CameraView2D>, IRenderTargetUser2D
		where TType : struct
	{
		protected GameLoop2D(TType type) : base(type)
		{
		}

		public void DrawTargets(SpriteBatch sb, float t)
		{
		}

		public virtual void Draw(SpriteBatch sb, float t)
		{
		}
	}
}

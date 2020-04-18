using Engine.Graphics._3D.Rendering;
using Engine.Interfaces._3D;
using Engine.View._3D;

namespace Engine.Loops
{
	public abstract class GameLoop3D<TType> : GameLoop<TType, Camera3D, CameraView3D>, IRenderTargetUser3D
		where TType : struct
	{
		protected GameLoop3D(TType type) : base(type)
		{
		}

		public MasterRenderer3D Renderer { get; internal set; }

		public virtual void DrawTargets(float t)
		{
			Renderer.DrawTargets(t);
		}

		public virtual void Draw(float t)
		{
			Renderer.Draw();
		}
	}
}

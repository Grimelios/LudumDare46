using Engine.Core._2D;

namespace Engine.Interfaces._2D
{
	public interface IRenderPositionable2D
	{
		RenderPosition2DField Position { get; }

		void Recompute(float t);
	}
}

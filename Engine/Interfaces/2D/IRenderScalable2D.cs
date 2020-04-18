using Engine.Core._2D;

namespace Engine.Interfaces._2D
{
	public interface IRenderScalable2D
	{
		RenderScale2DField Scale { get; }
		
		void Recompute(float t);
	}
}

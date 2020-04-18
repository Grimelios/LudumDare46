using Engine.Core._3D;

namespace Engine.Interfaces._3D
{
	public interface IRenderScalable3D
	{
		RenderScale3DField Scale { get; }
		
		void Recompute(float t);
	}
}

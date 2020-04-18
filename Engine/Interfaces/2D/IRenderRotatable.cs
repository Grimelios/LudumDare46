using Engine.Core._2D;

namespace Engine.Interfaces._2D
{
	public interface IRenderRotatable
	{
		RenderRotationField Rotation { get; }
		
		void Recompute(float t);
	}
}

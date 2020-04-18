using Engine.Core._3D;

namespace Engine.Interfaces._3D
{
	public interface IRenderOrientable
	{
		RenderOrientationField Orientation { get; }

		void Recompute(float t);
	}
}

using Engine.Core._3D;

namespace Engine.Interfaces._3D
{
	public interface IRenderPositionable3D
	{
		RenderPosition3DField Position { get; }
		
		void Recompute(float t);
	}
}

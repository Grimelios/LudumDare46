using Engine.Core;

namespace Engine.Interfaces
{
	public interface IRenderColorable
	{
		RenderColorField Color { get; }

		void Recompute(float t);
	}
}

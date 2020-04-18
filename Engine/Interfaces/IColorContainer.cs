using Engine.Core;

namespace Engine.Interfaces
{
	public interface IColorContainer
	{
		Color Color { get; }

		void SetColor(Color color, bool shouldInterpolate);
	}
}

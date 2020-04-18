using GlmSharp;

namespace Engine.Interfaces._2D
{
	public interface IPositionContainer2D
	{
		vec2 Position { get; }

		void SetPosition(vec2 position, bool shouldInterpolate);
	}
}

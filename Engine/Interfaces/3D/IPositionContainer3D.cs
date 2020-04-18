using GlmSharp;

namespace Engine.Interfaces._3D
{
	public interface IPositionContainer3D
	{
		vec3 Position { get; }

		void SetPosition(vec3 position, bool shouldInterpolate);
	}
}

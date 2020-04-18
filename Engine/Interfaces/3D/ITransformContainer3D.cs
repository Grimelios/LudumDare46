using GlmSharp;

namespace Engine.Interfaces._3D
{
	public interface ITransformContainer3D : IPositionContainer3D, IOrientationContainer
	{
		void SetTransform(vec3 position, quat orientation, bool shouldInterpolate);
	}
}

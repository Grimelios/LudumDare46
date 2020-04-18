using GlmSharp;

namespace Engine.Interfaces._3D
{
	public interface IScaleContainer3D
	{
		vec3 Scale { get; }

		void SetScale(vec3 scale, bool shouldInterpolate);
	}
}

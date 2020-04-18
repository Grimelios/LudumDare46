using GlmSharp;

namespace Engine.Interfaces._2D
{
	public interface IScaleContainer2D
	{
		vec2 Scale { get; }

		void SetScale(vec2 scale, bool shouldInterpolate);
	}
}

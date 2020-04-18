using GlmSharp;

namespace Engine.Interfaces._3D
{
	public interface IOrientationContainer
	{
		quat Orientation { get; }

		void SetOrientation(quat orientation, bool shouldInterpolate);
	}
}

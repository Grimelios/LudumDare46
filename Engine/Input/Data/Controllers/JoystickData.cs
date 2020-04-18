using GlmSharp;

namespace Engine.Input.Data.Controllers
{
	public class JoystickData
	{
		public JoystickData(vec2 vector, vec2 oldVector)
		{
			Vector = vector;
			OldVector = oldVector;
		}

		public vec2 Vector { get; }
		public vec2 OldVector { get; }
	}
}

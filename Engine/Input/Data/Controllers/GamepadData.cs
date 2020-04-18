using System.Diagnostics;
using Engine.Utility;

namespace Engine.Input.Data.Controllers
{
	// TODO: Add triggers.
	public class GamepadData : ControllerData
	{
		private static readonly int MaxButton = Utilities.EnumCount<XboxButtons>();
		
		public GamepadData(InputTypes type, InputStates[] buttons, JoystickData leftStick, JoystickData rightStick) :
			base(type, MaxButton, buttons, leftStick, rightStick)
		{
			Debug.Assert(type == InputTypes.Xbox360 || type == InputTypes.Xbone, "Invalid input type passed into " +
				"the gamepad constructor (must be Xbox-related).");
		}
	}
}

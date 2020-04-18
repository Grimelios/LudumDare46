using System.Diagnostics;
using Engine.Utility;

namespace Engine.Input.Data.Controllers
{
	public class DualshockData : ControllerData
	{
		private static readonly int MaxButton = Utilities.EnumCount<DualshockButtons>();

		public DualshockData(InputTypes type, InputStates[] buttons, JoystickData leftStick, JoystickData rightStick) :
			base(type, MaxButton, buttons, leftStick, rightStick)
		{
			Debug.Assert(type == InputTypes.DS3 || type == InputTypes.DS4, "Invalid input type passed into the " +
				"dualshock constructor (must be Playstation-related).");
		}
	}
}

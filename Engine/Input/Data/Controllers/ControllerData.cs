using System.Diagnostics;
using System.Linq;

namespace Engine.Input.Data.Controllers
{
	public abstract class ControllerData : InputData
	{
		private InputStates[] buttons;

		private JoystickData leftStick;
		private JoystickData rightStick;

		private int maxButton;

		protected ControllerData(InputTypes inputType, int maxButton, InputStates[] buttons, JoystickData leftStick,
			JoystickData rightStick) : base(inputType)
		{
			this.maxButton = maxButton;
			this.buttons = buttons;
			this.leftStick = leftStick;
			this.rightStick = rightStick;
		}

		public override InputStates this[int data]
		{
			get
			{
				Debug.Assert(data >= 0 && data <= maxButton, "Invalid button (outside maximum range).");

				return buttons[data];
			}
		}

		public JoystickData this[Joysticks stick] => stick == Joysticks.Left ? leftStick : rightStick;

		public override bool AnyPressed()
		{
			// For controllers, moving a joystick or using triggers doesn't count as a press (in this context).
			return buttons.Any(b => b == InputStates.PressedThisFrame);
		}

		public override bool Query(int data, InputStates state)
		{
			Debug.Assert(data >= 0 && data <= maxButton, "Invalid button (outside maximum range).");

			return false;
		}
	}
}

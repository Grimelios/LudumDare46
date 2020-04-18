using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Input.Data
{
	// TODO: Consider combining controller types (i.e. simplify to just Xbox and Dualshock).
	public enum InputTypes
	{
		All,
		Keyboard,
		Mouse,
		Xbox360,
		Xbone,
		DS3,
		DS4,
		Steam
	}
}

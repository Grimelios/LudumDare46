using System.Collections.Generic;
using System.Linq;
using Engine.Input.Data;
using static Engine.GLFW;

namespace Engine.Input
{
	internal class InputTarget
	{
		private IControllable target;

		public InputTarget(IControllable target)
		{
			this.target = target;

			MouseButtons = Enumerable.Repeat(InputStates.Released, GLFW_MOUSE_BUTTON_LAST).ToArray();
			Keys = Enumerable.Repeat(InputStates.Released, GLFW_KEY_LAST).ToArray();
			Buffers = new List<InputBuffer>();
		}

		public IControllable Target => target;
		public InputStates[] MouseButtons { get; set; }
		public InputStates[] Keys { get; set; }

		public List<InputBuffer> Buffers { get; }
	}
}

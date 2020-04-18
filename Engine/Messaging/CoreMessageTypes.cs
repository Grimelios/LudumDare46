using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Messaging
{
	public class CoreMessageTypes
	{
		// Using large values minimizes the chances of an accidental overlap with custom message types.
		public const int Exit = int.MaxValue;
		public const int Gamestate = int.MaxValue - 1;
		public const int ResizeRender = int.MaxValue - 2;
		public const int ResizeWindow = int.MaxValue - 3;
	}
}

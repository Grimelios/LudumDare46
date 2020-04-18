using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.UI.Objects
{
	public class Button
	{
		public Action Hover { get; set; }
		public Action Press { get; set; }
		public Action Release { get; set; }
	}
}

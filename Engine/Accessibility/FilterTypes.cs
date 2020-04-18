using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Accessibility
{
	public enum FilterTypes
	{
		// These numeric values match the ordering in the reference file (https://github.com/nvkelso/color-oracle-java/blob/master/src/ika/colororacle/Simulator.java).
		Deuteranopia = 0,
		Grayscale = 3,
		None = 4,
		Protanopia = 1,
		Tritanopia = 2
	}
}

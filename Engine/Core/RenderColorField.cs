using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Core
{
	public class RenderColorField : RenderField<Color>
	{
		public RenderColorField(Flags<TransformFlags> flags) : base(flags, TransformFlags.IsColorSet,
			TransformFlags.IsColorInterpolationNeeded, Color.White)
		{
		}

		protected override Color Interpolate(float t)
		{
			return Color.Lerp(OldValue, Value, t);
		}
	}
}

using GlmSharp;

namespace Engine.Core._2D
{
	public class RenderScale2DField : RenderField<vec2>
	{
		public RenderScale2DField(Flags<TransformFlags> flags) : base(flags, TransformFlags.IsScaleSet,
			TransformFlags.IsScaleInterpolationNeeded, vec2.Ones)
		{
		}

		protected override vec2 Interpolate(float t)
		{
			return vec2.Lerp(OldValue, Value, t);
		}

		public void SetX(float x, bool shouldInterpolate)
		{
			SetValue(new vec2(x, Value.y), shouldInterpolate);
		}

		public void SetY(float y, bool shouldInterpolate)
		{
			SetValue(new vec2(Value.x, y), shouldInterpolate);
		}
	}
}

using GlmSharp;

namespace Engine.Core._3D
{
	public class RenderScale3DField : RenderField<vec3>
	{
		public RenderScale3DField(Flags<TransformFlags> flags) : base(flags, TransformFlags.IsScaleSet,
			TransformFlags.IsScaleInterpolationNeeded, vec3.Ones)
		{
		}

		protected override vec3 Interpolate(float t)
		{
			return vec3.Lerp(OldValue, Value, t);
		}

		public void SetX(float x, bool shouldInterpolate)
		{
			SetValue(new vec3(x, Value.y, Value.z), shouldInterpolate);
		}

		public void SetY(float y, bool shouldInterpolate)
		{
			SetValue(new vec3(Value.x, y, Value.z), shouldInterpolate);
		}

		public void SetZ(float z, bool shouldInterpolate)
		{
			SetValue(new vec3(Value.x, Value.y, z), shouldInterpolate);
		}
	}
}

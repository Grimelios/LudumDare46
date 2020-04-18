using GlmSharp;

namespace Engine.Core._3D
{
	public class RenderOrientationField : RenderField<quat>
	{
		public RenderOrientationField(Flags<TransformFlags> flags) : base(flags, TransformFlags.IsOrientationSet,
			TransformFlags.IsOrientationInterpolationNeeded, quat.Identity)
		{
		}

		protected override quat Interpolate(float t)
		{
			var q = quat.SLerp(OldValue, Value, t);

			// Interpolation between quats can result in NaN if the two quats are nearly identical. Through testing,
			// this direct check is the only method that properly maintains smoothness (using a dot product with
			// epsilon didn't quite cut it).
			return float.IsNaN(q.w) ? Value : q;
		}
	}
}

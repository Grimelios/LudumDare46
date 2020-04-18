using Engine.Utility;

namespace Engine.Core._2D
{
	public class RenderRotationField : RenderField<float>
	{
		public RenderRotationField(Flags<TransformFlags> flags) : base(flags, TransformFlags.IsRotationSet,
			TransformFlags.IsRotationInterpolationNeeded, 0)
		{
		}

		protected override float Interpolate(float t)
		{
			return Utilities.Lerp(OldValue, Value, t);
		}
	}
}

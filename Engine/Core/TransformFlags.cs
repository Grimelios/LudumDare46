using System;

namespace Engine.Core
{
	// TODO: Consider supporting source interpolation (if needed). Almost certainly won't be needed.
	[Flags]
	public enum TransformFlags
	{
		IsColorInterpolationNeeded = 1 << 1,
		IsColorSet = 1 << 2,
		IsOrientationInterpolationNeeded = 1 << 3,
		IsOrientationSet = 1 << 4,
		IsPositionInterpolationNeeded = 1 << 5,
		IsPositionSet = 1 << 6,
		IsRotationInterpolationNeeded = 1 << 7,
		IsRotationSet = 1 << 8,
		IsScaleInterpolationNeeded = 1 << 9,
		IsScaleSet = 1 << 10,
		IsSourceChanged = 1 << 11,
		None = 0
	}
}

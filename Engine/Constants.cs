using System;

namespace Engine
{
	public static class Constants
	{
		internal const int DefaultSegments = 16;
		internal const int DefaultSpacing = 4;
		internal const int TextureLimit = 6;

		public const float DefaultEpsilon = 0.00001f;
		public const float Pi = (float)Math.PI;
		public const float PiOverTwo = Pi / 2;
		public const float PiOverFour = Pi / 4;
		public const float TwoPi = Pi * 2;

		public const ushort PrimitiveRestartIndex = 65535;
	}
}

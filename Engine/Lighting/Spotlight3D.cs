using System.Diagnostics;

namespace Engine.Lighting
{
	public class Spotlight3D : LightSource3D
	{
		private float spread;

		public float Spread
		{
			get => spread;
			set
			{
				Debug.Assert(value >= 0, "Spotlight spread can't be negative.");

				spread = value;
			}
		}
	}
}

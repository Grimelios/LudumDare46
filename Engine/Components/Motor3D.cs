using System.Diagnostics;
using Engine.Interfaces._3D;
using Engine.Utility;
using GlmSharp;

namespace Engine.Components
{
	public class Motor3D : Motor<IOrientationContainer>
	{
		private vec3 axis;
		private quat baseOrientation;

		public Motor3D(IOrientationContainer parent, vec3 axis, bool isRunning = true) : base(parent, isRunning)
		{
			// This assumes that the parent object is oriented correctly when the motor is created ("correct" meaning
			// its orientation if the motor was disabled with zero cumulative rotation).
			baseOrientation = parent.Orientation;
			Axis = axis;
		}

		public vec3 Axis
		{
			get => axis;
			set
			{
				Debug.Assert(Utilities.LengthSquared(value) > 0, "Motor axis must have non-zero length.");

				axis = value;
			}
		}

		protected override void Refresh(IOrientationContainer parent, float rotation)
		{
			parent.SetOrientation(baseOrientation * quat.FromAxisAngle(rotation, axis), true);
		}
	}
}

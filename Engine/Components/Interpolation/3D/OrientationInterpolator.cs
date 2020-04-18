using System.Diagnostics;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Components.Interpolation._3D
{
	public class OrientationInterpolator : Interpolator<quat>
	{
		private IOrientable target;
		private IRenderOrientable renderTarget;
		private IOrientationContainer containerTarget;

		public OrientationInterpolator(IOrientable target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, quat.Identity, quat.Identity, easeType, 0, isRepeatable)
		{
		}

		public OrientationInterpolator(IRenderOrientable target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, quat.Identity, quat.Identity, easeType, 0, isRepeatable)
		{
		}

		public OrientationInterpolator(IOrientationContainer target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, quat.Identity, quat.Identity, easeType, 0, isRepeatable)
		{
		}

		public OrientationInterpolator(IOrientable target, quat start, quat end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			this.target = target;
		}

		public OrientationInterpolator(IRenderOrientable target, quat start, quat end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			renderTarget = target;
		}

		public OrientationInterpolator(IOrientationContainer target, quat start, quat end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			containerTarget = target;
		}

		protected override void Lerp(float t)
		{
			var q = quat.SLerp(Start, End, t);

			if (target != null)
			{
				target.Orientation = q;
			}
			else if (renderTarget != null)
			{
				renderTarget.Orientation.SetValue(q, true);
			}
			else
			{
				containerTarget.SetOrientation(q, true);
			}
		}
	}
}

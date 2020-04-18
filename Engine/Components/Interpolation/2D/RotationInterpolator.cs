using System.Diagnostics;
using Engine.Interfaces._2D;
using Engine.Utility;

namespace Engine.Components.Interpolation._2D
{
	public class RotationInterpolator : Interpolator<float>
	{
		private IRotatable target;
		private IRenderRotatable renderTarget;
		private IRotationContainer containerTarget;

		public RotationInterpolator(IRotatable target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, 0, 0, easeType, 0, isRepeatable)
		{
		}

		public RotationInterpolator(IRenderRotatable target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, 0, 0, easeType, 0, isRepeatable)
		{
		}

		public RotationInterpolator(IRotationContainer target, EaseTypes easeType, bool isRepeatable = true) :
			this(target, 0, 0, easeType, 0, isRepeatable)
		{
		}

		public RotationInterpolator(IRotatable target, float start, float end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			this.target = target;
		}

		public RotationInterpolator(IRenderRotatable target, float start, float end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			renderTarget = target;
		}

		public RotationInterpolator(IRotationContainer target, float start, float end, EaseTypes easeType,
			float duration = 0, bool isRepeatable = true) : base(start, end, duration, easeType, isRepeatable)
		{
			Debug.Assert(target != null, NullMessage);

			containerTarget = target;
		}

		protected override void Lerp(float t)
		{
			var r = Utilities.Lerp(Start, End, t);

			if (target != null)
			{
				target.Rotation = r;
			}
			else if (renderTarget != null)
			{
				renderTarget.Rotation.SetValue(r, true);
			}
			else
			{
				containerTarget.SetRotation(r, true);
			}
		}
	}
}

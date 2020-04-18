using System.Diagnostics;
using Engine.Interfaces;

namespace Engine.Components.Interpolation
{
	// Interpolators are similar to timers, but I think it still makes sense to have them separate (primarily to more
	// easily modify start and end values mid-tick).
	public abstract class Interpolator<T> : IComponent where T : struct
	{
		protected const string NullMessage = "Can't interpolate on a null target.";

		private bool isRepeatable;

		private float duration;
		private float elapsed;

		private EaseTypes easeType;

		protected Interpolator(T start, T end, float duration, EaseTypes easeType, bool isRepeatable)
		{
			Debug.Assert(duration >= 0, "Interpolator duration can't be negative on construction.");

			this.duration = duration;
			this.easeType = easeType;
			this.isRepeatable = isRepeatable;
			
			Start = start;
			End = end;
			IsEnabled = !isRepeatable;
		}

		// It's possible to modify these values mid-interpolation.
		public T Start { get; set; }
		public T End { get; set; }

		public bool IsComplete { get; private set; }
		public bool IsEnabled { get; set; }

		public float Elapsed => elapsed;

		public float Duration
		{
			get => duration;
			set
			{
				Debug.Assert(value > 0, "Can't set interpolator duration to a non-positive value.");

				duration = value;
			}
		}

		public void Reset()
		{
			elapsed = 0;
			IsComplete = false;
		}

		protected abstract void Lerp(float t);

		// In some cases, it's useful to call this function directly.
		public void Interpolate(float t)
		{
			Lerp(Ease.Compute(t, easeType));
		}

		public void Update()
		{
			if (IsComplete || !IsEnabled)
			{
				return;
			}

			elapsed += DeltaTime.Value;

			if (elapsed >= duration)
			{
				// This ignores any leftover time (such that the end state is set exactly).
				Interpolate(1);

				if (isRepeatable)
				{
					Reset();
				}
				else
				{
					IsComplete = true;
				}

				return;
			}

			Interpolate(elapsed / duration);
		}
	}
}

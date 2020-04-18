using System;
using System.Diagnostics;

namespace Engine.Timing
{
	public class RepeatingTimer : Timer
	{
		private Func<float, bool> trigger;

		public RepeatingTimer(Func<float, bool> trigger, Action<float> tick = null, float duration = 0,
			TimerFlags flags = TimerFlags.IsPaused | TimerFlags.IsRepeatable, float elapsed = 0) :
			base(tick, duration, elapsed, flags)
		{
			Debug.Assert((flags & TimerFlags.ShouldResetOnRepeat) == 0, "The ShouldResetOnRepeat flag can't be used " +
				"with repeating timers (it doesn't logically make sense).");

			this.trigger = trigger;
		}

		public override void Update()
		{
			if (IsPaused || IsComplete)
			{
				return;
			}

			Debug.Assert(duration > 0, "Can't update a timer with a non-positive duration.");

			elapsed += DeltaTime.Value;

			while (elapsed >= duration && !IsPaused)
			{
				var previousDuration = duration;

				// If the trigger function is null, the repeating timer ends (otherwise you'd be stuck in an infinite
				// loop).
				if (trigger == null || !trigger.Invoke(elapsed % duration))
				{
					if (IsRepeatable)
					{
						Elapsed = 0;
						IsPaused = true;
					}
					else
					{
						IsComplete = true;
					}

					return;
				}

				elapsed -= previousDuration;
			}

			Tick?.Invoke(Elapsed / Duration);
		}
	}
}

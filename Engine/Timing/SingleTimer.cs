using System;
using System.Diagnostics;

namespace Engine.Timing
{
	public class SingleTimer : Timer
	{
		private bool shouldResetOnRepeat;

		public SingleTimer(Action<float> trigger = null, Action<float> tick = null, float duration = 0,
			TimerFlags flags = TimerFlags.IsPaused | TimerFlags.IsRepeatable | TimerFlags.ShouldResetOnRepeat,
			float elapsed = 0) : base(tick, duration, elapsed, flags)
		{
			Trigger = trigger;
			shouldResetOnRepeat = (flags & TimerFlags.ShouldResetOnRepeat) > 0;
		}

		// The argument is the leftover time (since the duration is unlikely to be hit exactly).
		public Action<float> Trigger { get; set; }

		public override void Update()
		{
			if (IsPaused || IsComplete)
			{
				return;
			}

			Debug.Assert(duration > 0, "Can't update a timer with a non-positive duration (this likely means that " +
				"duration was never set).");

			elapsed += DeltaTime.Value;

			if (elapsed >= duration)
			{
				// Calling the tick function here means that tick logic doesn't need to be duplicated in the trigger
				// function.
				Tick?.Invoke(1);

				// Elapsed time needs to be updated before the trigger function is called (in case the trigger sets
				// duration to a new value).
				if (IsRepeatable)
				{
					elapsed -= duration;
				}

				// It's considered valid for the trigger function to be null (primarly for tick-only timers).
				Trigger?.Invoke(elapsed);

				if (IsRepeatable)
				{
					if (shouldResetOnRepeat)
					{
						Elapsed = 0;
						IsPaused = true;
					}
				}
				else
				{
					IsComplete = true;
				}

				return;
			}

			Tick?.Invoke(elapsed / duration);
		}
	}
}

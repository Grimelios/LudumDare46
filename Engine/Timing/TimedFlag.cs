using System;
using System.Diagnostics;
using Engine.Interfaces;

namespace Engine.Timing
{
	// This class is very similar (or identical) to regular timers in many ways, but it still feels more appropriate to
	// use a new class. A timed flag is conceptually a specialized kind of timer.
	public class TimedFlag : IComponent
	{
		private float elapsed;
		private float duration;

		private bool defaultValue;
		private bool isPaused;

		public TimedFlag(bool defaultValue = false) : this(0, defaultValue)
		{
		}

		public TimedFlag(float duration, bool defaultValue = false)
		{
			Debug.Assert(duration >= 0, "Duration can't be negative.");

			this.duration = duration;
			this.defaultValue = defaultValue;

			isPaused = true;
		}

		// Flags are designed to be persistent as long as the parent object is in memory.
		public bool IsComplete => false;
		public bool IsEnabled => !isPaused;
		public bool Value => isPaused ? defaultValue : !defaultValue;

		public float Duration
		{
			get => duration;
			set
			{
				Debug.Assert(isPaused, "Can't modify a timed flag's duration while the flag is running.");
				Debug.Assert(value >= 0, "Timed flag duration can't be negative.");

				duration = value;
			}
		}

		// For flags that require tracking data, it often makes sense to track that data within the flag itself.
		public object Tag { get; set; }

		// Like single timers, the argument here is leftover time.
		public Action<float> OnExpiration { private get; set; }

		public void Refresh(object tag = null)
		{
			Tag = tag;

			// If a timed flag is refreshed while it's still running, the timer is simply reset to zero.
			if (!isPaused)
			{
				elapsed = 0;
			}
			else
			{
				isPaused = false;
			}
		}

		// This allows the flag to be reset without trigging the expiration callback.
		public void Reset()
		{
			elapsed = 0;
			isPaused = true;
		}

		public void Update()
		{
			if (isPaused)
			{
				return;
			}

			elapsed += DeltaTime.Value;

			if (elapsed >= duration)
			{
				OnExpiration?.Invoke(elapsed - duration);
				Reset();
			}
		}
	}
}

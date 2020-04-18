using System;
using System.Diagnostics;
using Engine.Interfaces;

namespace Engine.Timing
{
	public abstract class Timer : IComponent
	{
		protected float elapsed;
		protected float duration;
		protected TimerFlags flags;

		protected Timer(Action<float> tick, float duration, float elapsed, TimerFlags flags)
		{
			this.flags = flags;

			Tick = tick;
			Duration = duration;
			Elapsed = elapsed;
			IsPaused = (flags & TimerFlags.IsPaused) > 0;
			IsRepeatable = (flags & TimerFlags.IsRepeatable) > 0;
		}

		public float Elapsed
		{
			get => elapsed;
			set
			{
				Debug.Assert(value >= 0, "Timer elapsed can't be negative.");
				Debug.Assert(value <= Duration, "Timer elapsed can't be greater than duration.");

				elapsed = value;
			}
		}

		public float Duration
		{
			get => duration;
			set
			{
				Debug.Assert(value >= 0, "Timer duration can't be negative.");
				Debug.Assert(value >= elapsed, "Timer duration can't be less than the current elapsed time.");

				duration = value;
			}
		}

		public bool IsPaused { get; set; }
		public bool IsEnabled => !IsPaused;
		public bool IsRepeatable { get; set; }
		public bool IsComplete { get; protected set; }

		public Action<float> Tick { get; set; }

		public void Reset()
		{
			Elapsed = 0;
			IsPaused = true;
			IsComplete = false;
		}

		public abstract void Update();
	}
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Input.Data;

namespace Engine.Input
{
	public class InputBuffer
	{
		// Input buffers are designed to be conceptually bound to actions (i.e. lists of binds), not a specific bind.
		// For buffers that require a hold, though, the buffer is only considered satisfied if the *same* bind is held
		// (when the relevant event occurs).
		private Dictionary<InputBind, BindTimer> map;
		private List<InputBind> requiredChords;

		private float duration;

		internal InputBuffer(List<InputBind> binds, bool requiresHold, float duration)
		{
			map = new Dictionary<InputBind, BindTimer>();
			Duration = duration;
			RequiresHold = requiresHold;
			Binds = binds;
		}

		public float Duration
		{
			get => duration;
			set
			{
				Debug.Assert(value >= 0, "Input buffer duration can't be negative.");

				// If the new duration is longer, then all existing elapsed values can be kept intact. If shorter, some
				// buffered binds may need to be cut off.
				if (duration < value)
				{
					foreach (var tuple in map.Values.Where(tuple => tuple.Elapsed > value))
					{
						tuple.Reset();
					}
				}

				duration = value;
			}
		}

		// Some buffered actions require the bind to still be held when the action occurs.
		public bool RequiresHold { get; set; }

		public List<InputBind> Binds
		{
			set
			{
				Debug.Assert(value != null, "Can't set null binds on an input buffer.");

				// Refreshing the bind list only makes changes as necessary (existing binds are kept intact, assuming
				// they're also in the new list).
				foreach (var key in map.Select(pair => pair.Key).Where(key => !value.Contains(key)))
				{
					map.Remove(key);
				}

				foreach (var bind in value.Where(bind => !map.ContainsKey(bind)))
				{
					map.Add(bind, new BindTimer());
				}
			}
		}

		// This allows actions to be activated only when another bind is held (e.g. Ascend, which requires holding
		// another key when jump is pressed near an ascension target). For the time being, this works the same as Ori's
		// charge dash logic, where pressing both binds on the same frame also counts as a successful activation.
		public List<InputBind> RequiredChords
		{
			get => requiredChords;
			set
			{
				requiredChords = value;

				// For chord changes, it's simplest to just reset each buffer.
				foreach (var tuple in map.Values)
				{
					tuple.Reset();
				}
			}
		}

		// Manual resetting can happen on semi-rare occasions (such as Water Jumping, where a bind must be re-pressed
		// even if the same event technically occurs again within the buffer window of the first press).
		public void Reset()
		{
			foreach (var pair in map)
			{
				pair.Value.Reset();
			}
		}

		internal bool Refresh(FullInputData data)
		{
			foreach (var bind in map.Keys)
			{
				var state = data[bind];

				if (state == InputStates.PressedThisFrame)
				{
					var isChordSatisfied = RequiredChords == null || data.Query(RequiredChords, InputStates.Held);

					// By design, required chords are designed to work as modifiers on the base binds. In other words,
					// the action only triggers if a bind is pressed *while at least one of the chords is held*. This
					// also means that the chord could be released by the time the buffered action occurs, but I think
					// that's probably fine.
					if (isChordSatisfied)
					{
						OnPress(bind);
					}
				}
				else if (RequiresHold && state == InputStates.ReleasedThisFrame)
				{
					map[bind].Reset();
				}
				else
				{
					var timer = map[bind];

					if (!timer.IsPaused)
					{
						timer.Elapsed += DeltaTime.Value;

						if (timer.Elapsed > duration)
						{
							timer.Reset();
						}
					}
				}
			}

			return map.Values.Any(t => !t.IsPaused);
		}

		public bool Query()
		{
			return map.Any(t => !t.Value.IsPaused);
		}

		public bool Query(out InputBind bind)
		{
			bind = null;

			var min = float.MaxValue;

			foreach (var key in map.Keys)
			{
				var timer = map[key];

				// If multiple binds were successfully buffered, the bind that was pressed most recently is
				// returned.
				if (!timer.IsPaused && timer.Elapsed < min)
				{
					min = timer.Elapsed;
					bind = key;
				}
			}

			return bind != null;
		}

		private void OnPress(InputBind bind)
		{
			var tuple = map[bind];

			if (tuple.IsPaused)
			{
				tuple.IsPaused = false;
			}
			else
			{
				tuple.Elapsed = 0;
			}
		}

		private class BindTimer
		{
			public BindTimer()
			{
				IsPaused = true;
			}

			public bool IsPaused { get; set; }
			public float Elapsed { get; set; }

			public void Reset()
			{
				Elapsed = 0;
				IsPaused = true;
			}
		}
	}
}

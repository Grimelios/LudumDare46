using System.Diagnostics;
using System.Linq;
using static Engine.GLFW;

namespace Engine.Input.Data
{
	public class KeyboardData : InputData
	{
		private InputStates[] keys;

		public KeyboardData(InputStates[] keys, KeyPress[] keysPressedThisFrame) : base(InputTypes.Keyboard)
		{
			this.keys = keys;

			KeysPressedThisFrame = keysPressedThisFrame;
		}

		internal InputStates[] Keys
		{
			set => keys = value;
		}

		public KeyPress[] KeysPressedThisFrame { get; internal set; }

		public override InputStates this[int data]
		{
			get
			{
				Debug.Assert(data >= 0 && data <= GLFW_KEY_LAST, "Invalid key (outside maximum range).");

				return keys[data];
			}
		}

		public override bool AnyPressed()
		{
			return keys.Any(k => k == InputStates.PressedThisFrame);
		}

		public override bool Query(int data, InputStates state)
		{
			// This isn't a perfect assertion (since not all keys within this integer range are valid), but it's better
			// than nothing.
			Debug.Assert(data >= 0 && data <= GLFW_KEY_LAST, "Invalid key (outside maximum range).");

			return InputUtilities.Query(keys[data], state);
		}

		public bool Query(InputStates state, params int[] keys)
		{
			return keys.Any(k => Query(k, state));
		}

		// This returns the first key pressed in the given array.
		public bool Query(InputStates state, out int result, params int[] keys)
		{
			result = -1;

			foreach (var key in keys)
			{
				if (Query(key, state))
				{
					result = key;

					break;
				}
			}

			return result >= 0;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Input.Data;
using GlmSharp;
using static Engine.GLFW;

namespace Engine.Input
{
	// TODO: Disallow binding pause keys to other actions (much simpler that way).
	// TODO: If a device is disconnected while the game is running, any held binds need to be released.
	public class InputProcessor
	{
		// Normally I dislike singletons, but it makes sense in this case.
		internal static InputProcessor Instance { get; } = new InputProcessor();

		public static void Add(Action<FullInputData> function)
		{
			Debug.Assert(function != null, "Input function can't be null.");

			Instance.unorderedTargets.Add(function);
		}

		public static void Add(IControllable target, int order)
		{
			var targets = Instance.orderedTargets;

			Debug.Assert(target != null, "Input target can't be null.");
			Debug.Assert(order >= 0, "Input order must be non-negative.");
			Debug.Assert(targets.All(t => t.Order != order), $"Duplicate input order ({order}).");

			InputTarget existingTarget = null;

			// This loop accounts for targets that registered input buffers before adding themselves properly (via
			// this function).
			for (int i = 0; i < targets.Count; i++)
			{
				var (orderValue, t) = targets[i];

				if (orderValue >= 0)
				{
					break;
				}

				// If a temporary entry is found, that entry is removed (then re-inserted below with proper ordering).
				if (t.Target == target)
				{
					existingTarget = t;
					targets.RemoveAt(i);

					break;
				}
			}

			Debug.Assert(targets.All(t => t.Target.Target != target), "Duplicate input target.");
			
			var index = 0;

			while (index < targets.Count && targets[index].Order < order)
			{
				index++;
			}
			
			targets.Insert(index, (order, existingTarget ?? new InputTarget(target)));
		}

		public static void Remove(IControllable target)
		{
			Debug.Assert(target != null, "Can't remove a null input target.");

			var targets = Instance.orderedTargets;

			// In practice, the removed target will almost always be the top one. Still safer to code against other
			// cases.
			for (int i = 0; i < targets.Count; i++)
			{
				var t = targets[i];

				if (t.Target.Target == target)
				{
					targets.RemoveAt(i);

					return;
				}
			}
		}

		public static InputBuffer RegisterBuffer(IControllable target, List<InputBind> binds, bool requiresHold,
			float duration = 0)
		{
			var inputTarget = Instance.orderedTargets.FirstOrDefault(t => t.Target.Target == target).Target;

			// If untracked, the given target is temporarily tracked with an ordering of -1. This ordering is
			// overridden when the target is added normally (or else a debug assertion is thrown later). Doing this
			// allows targets to create input buffers before officially adding themselves to the processor.
			if (inputTarget == null)
			{
				inputTarget = new InputTarget(target);
				Instance.orderedTargets.Insert(0, (-1, inputTarget));
			}

			var buffer = new InputBuffer(binds, requiresHold, duration);

			inputTarget.Buffers.Add(buffer);

			return buffer;
		}

		// Targets are conceptually managed as a stack, but are implemented using a list so that earlier entries can be
		// removed if needed.
		private List<(int Order, InputTarget Target)> orderedTargets;
		private List<Action<FullInputData>> unorderedTargets;
		private List<KeyPress> keyPresses;

		private InputStates[] keys;
		private InputStates[] mouseButtons;

		// These is useful for handling blocks and unblocks (for both paused and unpaused variations).
		private InputTarget blockingPausedTarget;
		private InputTarget blockingUnpausedTarget;

		// TODO: Consider high-DPI mouse settings (see the GLFW input guide).
		// Mouse position is handled differently from other button/key presses (it's global among all targets). The
		// reason is that tracking mouse position per-target could result in artificially huge deltas if, for example,
		// the player pauses, moves the mouse far away, then unpauses. This could have unintended effects on any
		// gameplay mechanics based on mouse movement.
		private ivec2 mouseLocation;
		private ivec2 oldMouseLocation;

		// TODO: Also reset mouse delta when focus is lost and regained.
		// On the first frame of mouse movement, the mouse's previous location is artificially set to the current
		// location in order to avoid a large, false delta.
		private bool isFirstMouseMovement;

		private InputProcessor()
		{
			orderedTargets = new List<(int Order, InputTarget Target)>();
			unorderedTargets = new List<Action<FullInputData>>();
			keyPresses = new List<KeyPress>();
			keys = Enumerable.Repeat(InputStates.Released, GLFW_KEY_LAST).ToArray();
			mouseButtons = Enumerable.Repeat(InputStates.Released, GLFW_MOUSE_BUTTON_LAST).ToArray();
			isFirstMouseMovement = true;
		}

		internal void OnKeyPress(int key, int mods)
		{
			// See the comment for mouse button presses. This condition may not be necessary, but it's safer just in
			// case.
			if (InputUtilities.Query(keys[key], InputStates.Released))
			{
				keys[key] = InputStates.PressedThisFrame;
				keyPresses.Add(new KeyPress(key, (KeyModifiers)mods));
			}
			else
			{
				keys[key] = InputStates.Held;
			}
		}

		internal void OnKeyRelease(int key)
		{
			// See the comment for mouse button releases.
			keys[key] = InputUtilities.Query(keys[key], InputStates.Held)
				? InputStates.ReleasedThisFrame
				: InputStates.Released;
		}

		internal void OnMouseButtonPress(int button)
		{
			// As of writing this comment, I'm unsure if multiple (i.e. false) mouse presses can occur before a release
			// is processed. Still safer to code against it.
			mouseButtons[button] = InputUtilities.Query(mouseButtons[button], InputStates.Released)
				? InputStates.PressedThisFrame
				: InputStates.Held;
		}

		internal void OnMouseButtonRelease(int button)
		{
			// Mouse release can be falsely triggered twice if the mouse is held, then the game loses focus, then the
			// game *gains* focus (without releasing the mouse), *then* the mouse is actually released. In this
			// situation, the second release is ignored. I'm guessing this isn't the only scenario in which false
			// presses/releases can occur.
			mouseButtons[button] = InputUtilities.Query(mouseButtons[button], InputStates.Held)
				? InputStates.ReleasedThisFrame
				: InputStates.Released;
		}

		internal void OnMouseMove(float x, float y)
		{
			mouseLocation.x = (int)x;
			mouseLocation.y = (int)y;
		}

		// This is conceptually the same as a regular update, but using IDynamic would force Update to be public
		// (when I'd prefer it internal to the engine).
		internal void Refresh()
		{
			if (orderedTargets.Count == 0 && unorderedTargets.Count == 0)
			{
				return;
			}

			// TODO: Handle controller as well.
			var mouseData = GetMouseData(null);
			var keyboardData = GetKeyboardData(null);
			var fullData = new FullInputData();
			fullData.Set(InputTypes.Mouse, mouseData);
			fullData.Set(InputTypes.Keyboard, keyboardData);

			unorderedTargets.ForEach(t => t.Invoke(fullData));

			if (orderedTargets.Count == 0)
			{
				return;
			}

			Synchronize(null, orderedTargets[0].Target);

			// This is used when blocking (or unblocking) occurs under various conditions.
			void UpdateDataObjects(InputTarget target)
			{
				// TODO: Handle controller state as well.
				keyboardData.Keys = (InputStates[])target.Keys.Clone();
				keyboardData.KeysPressedThisFrame = new KeyPress[0];

				mouseData.Buttons = (InputStates[])target.MouseButtons.Clone();
			}

			// Targets are ordered in ascending order (such that the top input target is first).
			for (int i = 0; i < orderedTargets.Count; i++)
			{
				var target = orderedTargets[i].Target;
				var t = target.Target;
				var shouldBreak = false;

				target.Buffers.ForEach(b => b.Refresh(fullData));

				switch (t.ProcessInput(fullData))
				{
					case InputFlowTypes.BlockingPaused:
						if (blockingPausedTarget == null)
						{
							blockingPausedTarget = target;

							if (i < orderedTargets.Count - 1)
							{
								Synchronize(target, orderedTargets[i + 1].Target);
							}
						}
						else
						{
							shouldBreak = true;
						}

						break;

					case InputFlowTypes.BlockingUnpaused:
						shouldBreak = blockingUnpausedTarget != null;

						if (blockingUnpausedTarget == null && i < orderedTargets.Count - 1)
						{
							blockingUnpausedTarget = target;

							var child = orderedTargets[i + 1].Target;

							// When an unpaused block occurs, all currently-held buttons are simulated as released
							// (on the current frame).
							ReleaseAll(child);
							UpdateDataObjects(child);
						}

						break;

					case InputFlowTypes.Passthrough:
						var isPausedUnblock = blockingPausedTarget?.Target == t;
						var isUnpausedUnblock = blockingUnpausedTarget?.Target == t;

						if (i < orderedTargets.Count - 1)
						{
							var child = orderedTargets[i + 1].Target;

							// This means that a previously-blocking target has now allowed input to pass through (i.e.
							// the blocking target unblocked).
							if (isPausedUnblock)
							{
								// Note that when an unpause occurs, state is eventually transferred down to *all*
								// children (assuming a child doesn't block again first).
								Transfer(blockingPausedTarget, child);

								// When an unblock occurs, child state arrays will frequently be different, so data
								// objects need to be updated accordingly. Importantly, this also means that, when an
								// unpause occurs, the child *does* still process input on the same frame (although,
								// due to transfer logic, *presses this frame* don't exist for children).
								//
								// Additional note: data objects are only updated for the direct child of the
								// unblocking target. This should be safe under the reasoning that, when a pause
								// occurs, state would have previously passed through to all children (i.e. all locked
								// child state will already be synchronized, meaning that transferred state will *also*
								// be consistent).
								UpdateDataObjects(child);
							}
							else if (isUnpausedUnblock)
							{
								Transfer(blockingUnpausedTarget, child);
								UpdateDataObjects(child);
							}
							else
							{
								Synchronize(target, child);
							}
						}

						// Blocking targets need to be nullified even on the last input target.
						if (isPausedUnblock)
						{
							blockingPausedTarget = null;
						}
						else if (isUnpausedUnblock)
						{
							blockingUnpausedTarget = null;
						}

						break;
				}

				AdvanceStates(target);

				if (shouldBreak)
				{
					break;
				}
			}

			AdvanceStates(null);
		}

		private void Synchronize(InputTarget from, InputTarget to)
		{
			// TODO: Handle controller state as well.
			// This is technically wasteful (since the majority of keys won't change on a single frame). I considered
			// tracking deltas to fix this, but it's probably not worth it since these arrays are pretty small
			// anyway.
			Array.Copy(from?.Keys ?? keys, to.Keys, GLFW_KEY_LAST);
			Array.Copy(from?.MouseButtons ?? mouseButtons, to.MouseButtons, GLFW_MOUSE_BUTTON_LAST);
		}

		private KeyboardData GetKeyboardData(InputStates[] keys)
		{
			if (keys == null)
			{
				keys = this.keys;
			}

			var data = new KeyboardData((InputStates[])keys.Clone(), keyPresses.ToArray());

			keyPresses.Clear();

			return data;
		}

		private MouseData GetMouseData(InputStates[] buttons)
		{
			if (buttons == null)
			{
				buttons = mouseButtons;
			}

			if (isFirstMouseMovement && mouseLocation != ivec2.Zero)
			{
				oldMouseLocation = mouseLocation;
				isFirstMouseMovement = false;
			}

			var data = new MouseData(mouseLocation, oldMouseLocation, buttons);

			oldMouseLocation = mouseLocation;

			return data;
		}

		// This function is useful for handling unpaused blocks.
		private void ReleaseAll(InputTarget target)
		{
			void ReleaseAll(InputStates[] array)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (InputUtilities.Query(array[i], InputStates.Held))
					{
						array[i] = InputStates.ReleasedThisFrame;
					}
				}
			}

			// TODO: Handle controller state as well.
			ReleaseAll(target.Keys);
			ReleaseAll(target.MouseButtons);
		}

		private void AdvanceStates(InputTarget target)
		{
			void AdvanceStates(InputStates[] states)
			{
				for (int i = 0; i < states.Length; i++)
				{
					switch (states[i])
					{
						case InputStates.PressedThisFrame: states[i] = InputStates.Held; break;
						case InputStates.ReleasedThisFrame: states[i] = InputStates.Released; break;
					}
				}
			}

			// TODO: Handle controller state as well.
			AdvanceStates(target?.Keys ?? keys);
			AdvanceStates(target?.MouseButtons ?? mouseButtons);
		}

		private void Transfer(InputTarget from, InputTarget to)
		{
			void TransferTo(InputStates[] source, InputStates[] destination)
			{
				for (int i = 0; i < source.Length; i++)
				{
					// Presses and releases are not symmetric here. If a button was previously held, but released when
					// control is transferred, that counts as released this frame (when unpausing occurs). However, the
					// opposite scenario (a button previously released, then held as control is transferred) does *not*
					// count as pressed this frame (when unpausing occurs). The reasoning is that it'd feel weird to
					// hold a button (for potentially a long time) and have it still trigger an action immediately
					// (when unpausing occurs).
					switch (source[i])
					{
						// Important note: this logic means that it's possible to receive a "released this frame" state
						// without first receiving "pressed this frame" (if the button was released, then held during
						// a pause). As such, controllable classes (like the player) should code against this scenario.
						// It is NOT possible for fake "pressed this frame" signals to occur (although it's still
						// probably safer to code against them).
						case InputStates.Held:
						case InputStates.PressedThisFrame:
							destination[i] = source[i];

							break;

						case InputStates.Released:
						case InputStates.ReleasedThisFrame:
							destination[i] = InputUtilities.Query(destination[i], InputStates.Held)
								? InputStates.ReleasedThisFrame
								: InputStates.Released;

							break;
					}
				}
			}

			TransferTo(from.Keys, to.Keys);
			TransferTo(from.MouseButtons, to.MouseButtons);
		}
	}
}

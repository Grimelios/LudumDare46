using System;
using System.Diagnostics;
using System.Text;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Timing;
using static Engine.GLFW;

namespace Engine.Input.Processors
{
	public class TextProcessor : IDynamic
	{
		private static readonly char[] NumericSpecials = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };

		private StringBuilder builder;
		private SingleTimer repeatDelay;
		private RepeatingTimer repeatTimer;

		// Cursor position is before the character (i.e. position zero is the start of the line).
		private int cursor;
		private bool insertMode;

		private TextRepeatTypes repeatType;

		public TextProcessor()
		{
			builder = new StringBuilder();
			repeatDelay = new SingleTimer(t => repeatTimer.IsPaused = false);
			repeatTimer = new RepeatingTimer(t =>
			{
				switch (repeatType)
				{
					case TextRepeatTypes.Left:
						cursor--;

						if (cursor == 0)
						{
							return false;
						}

						break;

					case TextRepeatTypes.Right:
						cursor++;

						if (cursor == builder.Length)
						{
							return false;
						}

						break;

					case TextRepeatTypes.Backspace:
						return Backspace();

					case TextRepeatTypes.Delete:
						return Delete();
				}

				return true;
			});

			repeatType = TextRepeatTypes.None;
		}

		public string Value
		{
			get => builder.ToString();
			set
			{
				builder.Clear();
				builder.Append(value);

				// When value is set directly, the cursor is moved to the end of the line.
				cursor = value.Length;
			}
		}

		public int Cursor => cursor;

		public float RepeatDelay
		{
			set
			{
				Debug.Assert(value > 0, "Cursor repeat delay must be positive.");

				repeatDelay.Duration = value;
			}
		}

		public float RepeatRate
		{
			set
			{
				Debug.Assert(value > 0, "Cursor repeat rate must be positive.");

				// Repeat rate is given in characters per second.
				repeatTimer.Duration = 1f / value;
			}
		}

		public bool InsertMode => insertMode;

		public Func<string, bool> Submit { get; set; }

		public void ProcessKeyboard(KeyboardData data)
		{
			// If enter is pressed, all other text processing is ignored on the current frame.
			if (Value.Length > 0 && Submit != null && data.Query(InputStates.PressedThisFrame, GLFW_KEY_ENTER,
				GLFW_KEY_KP_ENTER))
			{
				ResetRepeat();

				if (Submit(Value))
				{
					builder.Clear();
					cursor = 0;
				}

				return;
			}

			// Simplest to just check these releases at the start of this function (rather than trying to optimize
			// key queries).
			if ((repeatType == TextRepeatTypes.Left && data.Query(GLFW_KEY_LEFT, InputStates.ReleasedThisFrame)) ||
				(repeatType == TextRepeatTypes.Right && data.Query(GLFW_KEY_RIGHT, InputStates.ReleasedThisFrame)) ||
				(repeatType == TextRepeatTypes.Backspace && data.Query(GLFW_KEY_BACKSPACE,
					InputStates.ReleasedThisFrame)) ||
				(repeatType == TextRepeatTypes.Delete && data.Query(GLFW_KEY_DELETE, InputStates.ReleasedThisFrame)))
			{
				ResetRepeat();
			}

			bool shift = data.Query(InputStates.Held, GLFW_KEY_LEFT_SHIFT, GLFW_KEY_RIGHT_SHIFT);
			bool capsLock = InputUtilities.IsEnabled(LockKeys.CapsLock);
			bool numLock = InputUtilities.IsEnabled(LockKeys.NumLock);

			// Even if numlock is off, special numpad functions can be activated by holding shift.
			if (!numLock)
			{
				numLock = shift;
			}

			bool home = data.Query(GLFW_KEY_HOME, InputStates.PressedThisFrame);
			bool end = data.Query(GLFW_KEY_END, InputStates.PressedThisFrame);
			bool left = data.Query(GLFW_KEY_LEFT, InputStates.PressedThisFrame);
			bool right = data.Query(GLFW_KEY_RIGHT, InputStates.PressedThisFrame);
			bool backspace = data.Query(GLFW_KEY_BACKSPACE, InputStates.PressedThisFrame);
			bool delete = data.Query(GLFW_KEY_DELETE, InputStates.PressedThisFrame);

			if (!numLock)
			{
				home |= data.Query(GLFW_KEY_KP_7, InputStates.PressedThisFrame);
				end |= data.Query(GLFW_KEY_KP_1, InputStates.PressedThisFrame);
				left |= data.Query(GLFW_KEY_KP_4, InputStates.PressedThisFrame);
				right |= data.Query(GLFW_KEY_KP_6, InputStates.PressedThisFrame);
				delete |= data.Query(GLFW_KEY_KP_DECIMAL, InputStates.PressedThisFrame);
			}

			// If home and end are pressed on the same frame, home takes priority.
			if (home)
			{
				cursor = 0;
			}
			else if (end)
			{
				cursor = builder.Length;
			}

			bool control = data.Query(InputStates.Held, GLFW_KEY_LEFT_CONTROL, GLFW_KEY_RIGHT_CONTROL);

			if (left ^ right)
			{
				// Left
				if (left)
				{
					if (cursor > 0)
					{
						if (control)
						{
							do
							{
								cursor--;
							}
							while (cursor > 0 && !(builder[cursor] != ' ' && builder[cursor - 1] == ' '));
						}
						else if (repeatType != TextRepeatTypes.None)
						{
							ResetRepeat();
						}
						else
						{
							cursor--;

							if (cursor > 0)
							{
								repeatDelay.IsPaused = false;
								repeatType = TextRepeatTypes.Left;
							}
						}
					}
				}
				// Right
				else if (cursor < builder.Length)
				{
					if (control)
					{
						do
						{
							cursor++;
						}
						while (cursor < builder.Length && !(builder[cursor - 1] == ' ' && builder[cursor] != ' '));
					}
					else if (repeatType != TextRepeatTypes.None)
					{
						ResetRepeat();
					}
					else
					{
						cursor++;

						if (cursor < builder.Length)
						{
							repeatDelay.IsPaused = false;
							repeatType = TextRepeatTypes.Right;
						}
					}
				}
			}

			// Backspace and delete are handled after moving the cursor and before adding new characters. Further,
			// both keys can be pressed on the same frame (and both are processed).
			if (backspace && builder.Length > 0 && cursor > 0)
			{
				// Backspace and delete can be repeated as well (and if so, they take priority over regular cursor
				// repeats).
				if (Backspace() && !delete)
				{
					repeatDelay.IsPaused = false;
					repeatType = TextRepeatTypes.Backspace;
				}
			}

			if (delete && builder.Length > 0 && cursor < builder.Length)
			{
				if (Delete() && !backspace)
				{
					repeatDelay.IsPaused = false;
					repeatType = TextRepeatTypes.Delete;
				}
			}

			bool insert = data.Query(GLFW_KEY_INSERT, InputStates.PressedThisFrame);

			// Changes to insert mode take effect immediately (e.g. pressing Insert and A on the same frame causes the
			// character at the current cursor position to be replaced).
			if (insert)
			{
				insertMode = !insertMode;
			}

			// Typing a new character cancels any ongoing repeat. Tracking cursor position is a simple way to check
			// this (rather than having to compare the full string to the old one).
			var oldCursor = cursor;

			foreach (var keyPress in data.KeysPressedThisFrame)
			{
				var character = GetCharacter(keyPress.Key, shift, capsLock, numLock);

				if (character.HasValue)
				{
					if (insertMode && cursor < builder.Length)
					{
						builder[cursor] = character.Value;
					}
					else
					{
						builder.Insert(cursor, character.Value);
					}

					cursor++;
				}
			}

			// Repeat logic can be canceled using a number of key presses (largely meant to mimic how existing editors
			// like Visual Studio behave).
			if (home || end || insert || control || cursor != oldCursor)
			{
				ResetRepeat();
			}
		}

		private bool Backspace()
		{
			builder.Remove(cursor - 1, 1);
			cursor--;

			return builder.Length > 0 && cursor > 0;
		}

		private bool Delete()
		{
			builder.Remove(cursor, 1);

			return builder.Length > 0 && cursor < builder.Length;
		}

		private char? GetCharacter(int key, bool shift, bool capsLock, bool numLock)
		{
			var c = (char)key;

			if (key >= GLFW_KEY_A && key <= GLFW_KEY_Z)
			{
				if (capsLock)
				{
					shift = !shift;
				}

				return shift ? c : char.ToLower(c);
			}

			if (key >= GLFW_KEY_0 && key <= GLFW_KEY_9)
			{
				return !shift ? c : NumericSpecials[c - '0'];
			}

			// Special numpad functions (like moving the cursor) are handled before reaching this point.
			if (numLock)
			{
				if (key >= GLFW_KEY_KP_0 && key <= GLFW_KEY_KP_9)
				{
					return (char)(c - (GLFW_KEY_KP_0 - GLFW_KEY_0));
				}

				if (key == GLFW_KEY_KP_DECIMAL)
				{
					return '.';
				}
			}

			switch (key)
			{
				case GLFW_KEY_COMMA: return shift ? '<' : ',';
				case GLFW_KEY_PERIOD: return shift ? '>' : '.';
				case GLFW_KEY_SLASH: return shift ? '?' : '/';
				case GLFW_KEY_SEMICOLON: return shift ? ':' : ';';
				case GLFW_KEY_APOSTROPHE: return shift ? '"' : '\'';
				case GLFW_KEY_LEFT_BRACKET: return shift ? '{' : '[';
				case GLFW_KEY_RIGHT_BRACKET: return shift ? '}' : ']';
				case GLFW_KEY_BACKSLASH: return shift ? '|' : '\\';
				case GLFW_KEY_MINUS: return shift ? '_' : '-';
				case GLFW_KEY_EQUAL: return shift ? '+' : '=';
				case GLFW_KEY_GRAVE_ACCENT: return shift ? '~' : '`';

				// These keys work even if numlock is turned off.
				case GLFW_KEY_KP_ADD: return '+';
				case GLFW_KEY_KP_SUBTRACT: return '-';
				case GLFW_KEY_KP_MULTIPLY: return '*';
				case GLFW_KEY_KP_DIVIDE: return '/';

				case GLFW_KEY_SPACE: return ' ';
			}

			return null;
		}

		private void ResetRepeat()
		{
			if (!repeatDelay.IsPaused)
			{
				repeatDelay.Reset();
			}
			else if (!repeatTimer.IsPaused)
			{
				repeatTimer.Reset();
			}

			repeatType = TextRepeatTypes.None;
		}

		public void Update()
		{
			if (!repeatDelay.IsPaused)
			{
				repeatDelay.Update();
			}
			else
			{
				repeatTimer.Update();
			}
		}
	}
}

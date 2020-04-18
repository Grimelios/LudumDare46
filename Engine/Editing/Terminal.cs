using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Engine.Content;
using Engine.Core;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Input;
using Engine.Input.Data;
using Engine.Input.Processors;
using Engine.Messaging;
using Engine.Props;
using Engine.UI;
using GlmSharp;
using static Engine.GLFW;

namespace Engine.Editing
{
	// TODO: Apply line limit.
	// TODO: Add scrolling.
	// TODO: Add italics (for usage of required commands).
	public class Terminal : CanvasElement, IControllable, IReceiver
	{
		private const int BackIndex = 0;
		private const int TextIndex = 1;
		private const int CursorIndex = 2;
		private const int SuccessIndex = 3;
		private const int FailureIndex = 4;

		private Primitive<bool> isPauseToggled;
		private SortedDictionary<string, Command> commands;
		private TextProcessor textProcessor;
		private Autocomplete autocomplete;
		private Color[] colors;
		private SpriteFont font;
		private SpriteText currentLine;
		private SpriteText suggestionText;
		private List<SpriteText> lines;
		private List<string> history;
		private Bounds2D insertBounds;

		// The terminal uses a monospace font.
		private int charWidth;
		private int padding;
		private int historyLimit;
		private int storedIndex;

		// This is the full suggestion string (not just what's displayed as autocomplete text).
		private string suggestion;

		public Terminal(Primitive<bool> isPauseToggled)
		{
			this.isPauseToggled = isPauseToggled;

			commands = new SortedDictionary<string, Command>();
			autocomplete = new Autocomplete();
			font = ContentCache.GetFont("Debug");
			currentLine = new SpriteText(font);
			suggestionText = new SpriteText(font);
			lines = new List<SpriteText>();
			history = new List<string>();
			charWidth = font.Measure("A").x;
			storedIndex = -1;
			insertBounds = new Bounds2D(charWidth, font.Size);

			var accessor = Properties.Access();

			padding = accessor.GetInt("terminal.padding");
			historyLimit = accessor.GetInt("terminal.history.limit");
			Height = accessor.GetInt("terminal.default.height");
			Anchor = Anchors.Left | Anchors.Top;
			IsDrawEnabled = false;
			colors = new Color[5];

			textProcessor = new TextProcessor();
			textProcessor.RepeatDelay = accessor.GetFloat("terminal.repeat.delay");
			textProcessor.RepeatRate = accessor.GetFloat("terminal.repeat.rate");
			textProcessor.Submit = Submit;

			AddDefaultCommands();

			MessageSystem.Subscribe(this, (int)CoreMessageTypes.ResizeWindow, data =>
			{
				// The terminal is always resized to fit the window width.
				Width = ((ivec2)data).x;
			});

			InputProcessor.Add(this, 10);
		}

		// These are temporary to get the build working.
		private int Height { get; set; }
		private int Width { get; set; }

		public List<MessageHandle> MessageHandles { get; set; }

		// TODO: Add other default commands (like "help" and "commands").
		private void AddDefaultCommands()
		{
			foreach (var command in Properties.GetCommands())
			{
				Add(command);
			}

			// TODO: Re-add the exit command.
			/*
			// Exit command (alias 'quit').
			Add((string[] args, out string result) =>
			{
				result = null;

				MessageSystem.Send(CoreMessageTypes.Exit);

				return true;
			}, "exit", "quit");
			*/
		}

		protected internal override void Initialize(Canvas canvas)
		{
			currentLine.Position.SetValue(new vec2(padding, Height - padding - font.Size), false);

			base.Initialize(canvas);
		}

		public override bool Reload(PropertyAccessor accessor, out string message)
		{
			// Reload colors.
			var keys = new []
			{
				"terminal.back.color",
				"terminal.text.color",
				"terminal.success.color",
				"terminal.failure.color",
				"terminal.suggestion.color"
			};
			
			if (!accessor.TryGetColors(keys, out var results, out message, this))
			{
				return false;
			}

			var textColor = results["terminal.text.color"];

			colors[BackIndex] = results["terminal.back.color"];
			colors[TextIndex] = textColor;
			colors[SuccessIndex] = results["terminal.success.color"];
			colors[FailureIndex] = results["terminal.failure.color"];

			currentLine.Color.SetValue(textColor, false);
			suggestionText.Color.SetValue(results["terminal.suggestion.color"], false);

			// Reload cursor opacity.
			if (!accessor.TryGetByte("terminal.cursor.opacity", out var opacity, out message, this))
			{
				return false;
			}

			var color = colors[TextIndex];
			color.A = opacity;
			colors[CursorIndex] = color;
			
			message = null;

			return true;
		}

		public InputFlowTypes ProcessInput(FullInputData data)
		{
			// TODO: Consider adding an X in the upper-right corner to exit using the mouse.
			if (data.TryGetData(out KeyboardData keyboard))
			{
				ProcessKeyboard(keyboard);
			}

			return IsDrawEnabled ? InputFlowTypes.BlockingUnpaused : InputFlowTypes.Passthrough;
		}

		public void Add(Command command)
		{
			Debug.Assert(command != null, "Command can't be null.");

			foreach (var key in command.Keywords)
			{
				Debug.Assert(!commands.ContainsKey(key), $"Duplicate command '{key}'.");

				Add($"Command '{key}' added.", colors[TextIndex]);
				commands.Add(key, command);
			}
		}

		public void Remove(Command command)
		{
			// This means that the command was never added. Shouldn't happen in practice, but also not worth stopping
			// the program for.
			if (commands.ContainsValue(command))
			{
				return;
			}

			foreach (var key in command.Keywords)
			{
				commands.Remove(key);
			}
		}

		private void ProcessKeyboard(KeyboardData data)
		{
			if (data.Query(GLFW_KEY_GRAVE_ACCENT, InputStates.PressedThisFrame))
			{
				IsDrawEnabled = !IsDrawEnabled;
				isPauseToggled = true;
				
				return;
			}

			if (!IsDrawEnabled)
			{
				return;
			}

			// Tab completion takes priority over cycling through previous commands (assuming a new, valid match is
			// found).
			if (data.Query(GLFW_KEY_TAB, InputStates.PressedThisFrame))
			{
				ProcessAutocomplete();
			}

			// Cycle through old commands.
			bool up = data.Query(GLFW_KEY_UP, InputStates.PressedThisFrame);
			bool down = data.Query(GLFW_KEY_DOWN, InputStates.PressedThisFrame);

			if (up ^ down)
			{
				string text = null;

				if (up && storedIndex < history.Count - 1)
				{
					storedIndex++;
					text = history[history.Count - storedIndex - 1];
				}
				else if (down && storedIndex > -1)
				{
					storedIndex--;
					text = storedIndex >= 0 ? history[history.Count - storedIndex - 1] : "";
				}

				// If an old command is selected, other keys are ignored on the current frame.
				if (text != null)
				{
					currentLine.Value = text;
					textProcessor.Value = text;
					autocomplete.Invalidate();

					return;
				}
			}

			var oldValue = textProcessor.Value;
			var oldCursor = textProcessor.Cursor;

			textProcessor.ProcessKeyboard(data);

			var newValue = textProcessor.Value;

			if (oldValue != newValue)
			{
				autocomplete.Invalidate();

				// Pressing space with a suggestion visible completes the suggestion (and adds a space).
				if (suggestionText.Length > 0 && newValue == oldValue + " ")
				{
					var index = oldValue.LastIndexOf(' ');

					currentLine.Value = (index == -1 ? suggestion : $"{oldValue.Substring(0, index)} {suggestion}")
						+ " ";
					textProcessor.Value = currentLine.Value;
					suggestionText.Value = null;

					return;
				}

				currentLine.Value = newValue;

				if (oldCursor == oldValue.Length && newValue.Length > oldValue.Length)
				{
					// Similar to Intellisense, suggestions are auto-filled while typing (but only while adding additional
					// characters, not when backspacing or deleting).
					if (!ProcessAutocomplete())
					{
						suggestionText.Value = null;
					}
				}
				else
				{
					suggestionText.Value = null;
				}
			}
		}

		private bool ProcessAutocomplete()
		{
			// Get options.
			string[] options = null;

			var raw = textProcessor.Value;
			var tokens = raw.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
			var endsWithSpace = raw.Length > 0 && raw.Last() == ' ';

			if (raw.Length == 0 || (tokens.Count == 1 && !endsWithSpace))
			{
				options = commands.Keys.ToArray();
			}
			else if (commands.TryGetValue(tokens[0], out var command))
			{
				var args = new List<string>();
				var limit = tokens.Count - (endsWithSpace ? 1 : 2);

				for (int i = 0; i < limit; i++)
				{
					args.Add(tokens[i + 1]);
				}

				options = command.GetOptions(args.ToArray());
			}

			// Process suggestions.
			if (options != null && options.Length > 0)
			{
				string s = raw.Length == 0 || endsWithSpace ? "" : tokens.Last();

				if (autocomplete.TryAutocomplete(s, options, out suggestion) && suggestion != s)
				{
					suggestionText.Value = suggestion.Substring(s.Length);
					suggestionText.Position.SetValue(currentLine.Position.Value +
						new vec2(charWidth * currentLine.Length, 0), false);

					return true;
				}
			}

			return false;
		}

		private bool Submit(string value)
		{
			var success = Run(value, out var result);

			// A null result string can be returned in rare cases (such as when exiting the program).
			if (success && result == null)
			{
				return true;
			}

			var color = success ? colors[SuccessIndex] : colors[FailureIndex];

			Add(result, color);

			return true;
		}

		private void Add(string s, Color color)
		{
			var line = new SpriteText(font, s);

			line.Color.SetValue(color, false);
			lines.Add(line);

			var start = new vec2(padding, Height - font.Size * 2 - padding * 3 - 3);
			var spacing = new vec2(0, font.Size + padding);

			for (int i = lines.Count - 1; i >= 0; i--)
			{
				lines[i].Position.SetValue(start - spacing * (lines.Count - i - 1), false);
			}
		}

		// TODO: This was made public for manual testing. Could be made private again in the future.
		public bool Run(string input, out string result)
		{
			var tokens = input.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var key = tokens[0];

			if (history.Count == historyLimit)
			{
				history.RemoveAt(0);
			}

			history.Add(input);
			storedIndex = -1;

			if (!commands.TryGetValue(key, out var command))
			{
				result = $"Unrecognized command '{key}'.";

				return false;
			}

			var usage = command.Usage;

			bool isValid;

			if (usage == null)
			{
				isValid = tokens.Length == 1;
			}
			else
			{
				var required = usage.Count(a => a.Type == ArgumentTypes.Required);

				// Each command's usage array does not contain the keyword itself (just arguments).
				isValid = tokens.Length >= required + 1 && tokens.Length <= usage.Length + 1;
			}

			if (!isValid)
			{
				if (usage == null)
				{
					result = "Usage: " + key;
				}
				else
				{
					var builder = new StringBuilder();

					foreach (var argument in usage)
					{
						var name = argument.Name;

						builder.Append(argument.Type == ArgumentTypes.Required
							? $" *{name}*"
							: $" [{name}]");
					}

					result = $"Usage: {key}{builder}";
				}

				return false;
			}

			var args = new string[tokens.Length - 1];

			Array.Copy(tokens, 1, args, 0, args.Length);

			return command.Process(args, out result);
		}

		public override void Dispose()
		{
			MessageSystem.Unsubscribe(this);
		}

		public override void Update()
		{
			var oldLength = textProcessor.Value.Length;

			textProcessor.Update();

			var value = textProcessor.Value;

			if (oldLength != value.Length)
			{
				currentLine.Value = value;
				autocomplete.Invalidate();
			}

			base.Update();
		}

		public override void Draw(SpriteBatch sb, float t)
		{
			var p = currentLine.Position.Value + new vec2(charWidth * textProcessor.Cursor + 1, 0);
			var l1 = new vec2(0, Height - font.Size - padding * 2 - 1);
			var l2 = l1 + new vec2(Width, 0);
			var cursorColor = colors[CursorIndex];

			//sb.Fill(bounds, colors[BackIndex]);

			// Draw the text cursor.
			if (textProcessor.InsertMode)
			{
				insertBounds.Location = (ivec2)p;
				sb.Fill(insertBounds, cursorColor);
			}
			else
			{
				sb.DrawLine(p, p + new vec2(0, font.Size), cursorColor);
			}

			sb.DrawLine(l1, l2, colors[TextIndex]);

			lines.ForEach(l => l.Draw(sb, t));
			currentLine.Draw(sb, t);
			suggestionText.Draw(sb, t);
		}
	}
}

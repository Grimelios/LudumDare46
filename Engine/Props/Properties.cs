using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Engine.Editing;
using Engine.Interfaces;
using Engine.Props.Commands;

namespace Engine.Props
{
	// TODO: Consider adding a function to remove a target (and its associated properties).
	public static class Properties
	{
		private static Dictionary<string, string> map;
		private static PropertyAccessor accessor;

		static Properties()
		{
			map = new Dictionary<string, string>();
			accessor = new PropertyAccessor(map);

			if (Directory.Exists(Paths.Properties))
			{
				foreach (var file in Directory.GetFiles(Paths.Properties, "*", SearchOption.AllDirectories))
				{
					Reload(file);
				}
			}
		}

		public static Command[] GetCommands()
		{
			var sortedKeys = map.Keys.ToArray();

			Array.Sort(sortedKeys);

			return new Command[]
			{
				new EchoCommand(map, sortedKeys),
				new SetCommand(map, accessor, sortedKeys) 
			};
		}

		private static void Reload(string filename)
		{
			var lines = File.ReadAllLines(filename);

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];

				// Comments start with '#'.
				if (line.Length == 0 || line[0] == '#')
				{
					continue;
				}

				Debug.Assert(!line.StartsWith("//"), $"Invalid property ('{filename}', line {i}'). Use # for " +
					"comments.");
				Debug.Assert(!line.EndsWith(";"), $"Invalid property ('{filename}', line {i}'). Don't end lines " +
					"with semicolons.");

				// The expected format of each line is "key = value" (although it'll work without the spaces as well).
				var tokens = line.Split('=');

				Debug.Assert(tokens.Length == 2, $"Invalid property ('{filename}', line {i}'). Expected format is " +
					"'key = value'.");

				var key = tokens[0].TrimEnd();
				var value = tokens[1].TrimStart();

				map.Add(key, value);
			}
		}

		public static PropertyAccessor Access(IReloadable target = null)
		{
			if (target != null && !target.Reload(accessor, out var message))
			{
				// By triggering a failure here, bad properties will be detected on object creation (rather than only
				// on reload via the terminal).
				Debug.Fail(message);
			}
			
			return accessor;
		}
	}
}

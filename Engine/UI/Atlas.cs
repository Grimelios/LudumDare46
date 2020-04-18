using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Engine.Core._2D;

namespace Engine.UI
{
	public class Atlas
	{
		private Dictionary<string, SourceRect> map;

		public Atlas(string filename)
		{
			var lines = File.ReadAllLines(Paths.Content + filename);

			map = new Dictionary<string, SourceRect>();

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];

				// Atlas comments start with # (just like properties).
				if (line.Length == 0 || line[0] == '#')
				{
					continue;
				}

				// The expected format of each atlas line is key = bounds, where bounds is a pipe-separated list of
				// values (X|Y|Width|Height).
				var tokens = line.Split('=');

				Debug.Assert(tokens.Length == 2, $"Incorrect atlas format in file {filename}, line {i + 1} " +
					"(expected 'key = X|Y|Width|Height'.");

				var key = tokens[0].TrimEnd();
				var rect = ParseSourceRect(tokens[1].TrimStart());

				map.Add(key, rect);
			}
		}

		public SourceRect this[string key] => map[key];

		private SourceRect ParseSourceRect(string s)
		{
			// The expected format is "X|Y|Width|Height".
			var tokens = s.Split('|');

			Debug.Assert(tokens.Length == 4, "Incorrect bounds format (expected X|Y|Width|Height).");

			var x = int.Parse(tokens[0]);
			var y = int.Parse(tokens[1]);
			var width = int.Parse(tokens[2]);
			var height = int.Parse(tokens[3]);

			return new SourceRect(x, y, width, height);
		}
	}
}

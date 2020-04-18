using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Engine.Editing
{
	internal class Autocomplete
	{
		private string[] lastMatches;
		private int lastIndex;

		public Autocomplete()
		{
			lastIndex = -1;
		}

		public bool TryAutocomplete(string s, string[] options, out string result)
		{
			Debug.Assert(options != null && options.Length > 0, "Terminal autocomplete should only be called if " +
				"options exist.");

			// TODO: Is it possible for options to change while cycling matches? (probably not, or it could be asserted)
			// This indicates that matches should cycle (i.e. text wasn't modified since the last autocomplete
			// attempt).
			if (lastIndex >= 0)
			{
				lastIndex = lastIndex < lastMatches.Length - 1 ? ++lastIndex : 0;
				result = lastMatches[lastIndex];

				return true;
			}
			
			Array.Sort(options);

			string[] matches;

			if (s.Length == 0)
			{
				matches = options;
			}
			else
			{
				matches = options.Where(option => option.Length > s.Length &&
					option.StartsWith(s, true, CultureInfo.CurrentCulture)).ToArray();

				if (matches.Length == 0)
				{
					result = null;

					return false;
				}
			}
			
			result = matches[0];
			lastMatches = matches;
			lastIndex = 0;

			return true;
		}

		// If tab is pressed multiple times in a row, matching options are cycled (similar to a real bash terminal).
		// Autocomplete attempts will continue to cycle in this way until this function is called.
		public void Invalidate()
		{
			lastMatches = null;
			lastIndex = -1;
		}
	}
}

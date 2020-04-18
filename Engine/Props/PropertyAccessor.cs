using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Core;
using Engine.Interfaces;
using Engine.Utility;

namespace Engine.Props
{
	public class PropertyAccessor
	{
		private Dictionary<string, string> map;
		private Dictionary<(string Key, PropertyTypes Type), List<IReloadable>> tracker;

		// These strings are used to reduce repetitive boilerplate in other classes (used to set message strings based
		// on common validation types).
		private string[] validationStrings;

		internal PropertyAccessor(Dictionary<string, string> map)
		{
			this.map = map;

			tracker = new Dictionary<(string Key, PropertyTypes Type), List<IReloadable>>();

			validationStrings = new string[Utilities.EnumCount<PropertyConstraints>()];
			validationStrings[(int)PropertyConstraints.NonNegative] = "non-negative";
			validationStrings[(int)PropertyConstraints.Positive] = "positive";
			validationStrings[(int)PropertyConstraints.ZeroToOne] = "between zero and one (inclusive)";
		}

		internal Dictionary<(string Key, PropertyTypes Type), List<IReloadable>> Tracker => tracker;

		public byte GetByte(string key)
		{
			Track(key, PropertyTypes.Byte, null);

			if (!byte.TryParse(map[key], out var result))
			{
				Debug.Fail(Format(key, PropertyTypes.Byte));
			}

			return result;
		}

		public bool TryGetByte(string key, out byte result, out string message, IReloadable target = null)
		{
			Track(key, PropertyTypes.Byte, target);

			if (!byte.TryParse(map[key], out result))
			{
				message = Format(key, PropertyTypes.Byte);

				return false;
			}

			// Byte constraints are implied (they must be a positive value between 0 and 255 inclusive).
			message = null;

			return true;
		}

		public bool TryGetBytes(string[] keys, out Dictionary<string, byte> results, out string message,
			IReloadable target = null)
		{
			results = new Dictionary<string, byte>();

			foreach (var key in keys)
			{
				if (!TryGetByte(key, out var result, out message, target))
				{
					return false;
				}

				results.Add(key, result);
			}

			message = null;

			return true;
		}

		public int GetInt(string key, PropertyConstraints constraint = PropertyConstraints.Positive)
		{
			Track(key, PropertyTypes.Integer, null);

			if (!int.TryParse(map[key], out var result))
			{
				Debug.Fail(Format(key, PropertyTypes.Integer));
			}

			if (!Validate(result, constraint))
			{
				Debug.Fail(Format(key, constraint));
			}

			return result;
		}
		
		public bool TryGetInt(string key, PropertyConstraints constraint, out int result, out string message,
			IReloadable target = null)
		{
			Track(key, PropertyTypes.Integer, target);

			if (!int.TryParse(map[key], out result))
			{
				message = Format(key, PropertyTypes.Integer);

				return true;
			}

			if (Validate(result, constraint))
			{
				message = null;

				return true;
			}
			
			message = Format(key, constraint);

			return false;
		}

		public bool TryGetInt(string key, PropertyConstraints constraint, out string message, ref int result,
			IReloadable target = null)
		{
			if (!TryGetInt(key, constraint, out var value, out message, target))
			{
				return false;
			}

			result = value;

			return true;
		}

		public bool TryGetInts(string[] keys, PropertyConstraints constraint, out Dictionary<string, int> results,
			out string message, IReloadable target = null)
		{
			results = new Dictionary<string, int>();

			foreach (var key in keys)
			{
				if (!TryGetInt(key, constraint, out var result, out message, target))
				{
					return false;
				}

				results.Add(key, result);
			}

			message = null;

			return true;
		}

		public uint GetUInt(string key)
		{
			Track(key, PropertyTypes.UnsignedInteger, null);

			if (!uint.TryParse(map[key], out var result))
			{
				Debug.Fail(Format(key, PropertyTypes.UnsignedInteger));
			}

			return result;
		}

		public bool TryGetUInt(string key, PropertyConstraints constraint, out uint result, out string message,
			IReloadable target = null)
		{
			Track(key, PropertyTypes.UnsignedInteger, target);

			if (!uint.TryParse(map[key], out result))
			{
				message = Format(key, PropertyTypes.UnsignedInteger);

				return true;
			}

			// The signed integer validator can be reused here. Using a constraint of NonNegative doesn't really make
			// sense for an unsigned integer (since that's always true), but it's also not an error.
			if (Validate((int)result, constraint))
			{
				message = null;

				return true;
			}

			message = Format(key, constraint);

			return false;
		}

		public bool TryGetUInt(string key, PropertyConstraints constraint, out string message, ref uint result,
			IReloadable target = null)
		{
			if (!TryGetUInt(key, constraint, out var value, out message, target))
			{
				return false;
			}

			result = value;

			return true;
		}

		public bool TryGetUInts(string[] keys, PropertyConstraints constraint, out Dictionary<string, uint> results,
			out string message, IReloadable target = null)
		{
			results = new Dictionary<string, uint>();

			foreach (var key in keys)
			{
				if (!TryGetUInt(key, constraint, out var result, out message, target))
				{
					return false;
				}

				results.Add(key, result);
			}

			message = null;

			return true;
		}

		public float GetFloat(string key, PropertyConstraints constraint = PropertyConstraints.Positive)
		{
			Track(key, PropertyTypes.Float, null);

			if (!float.TryParse(map[key], out var result))
			{
				Debug.Fail(Format(key, PropertyTypes.Float));
			}

			if (!Validate(result, constraint))
			{
				Debug.Fail(Format(key, constraint));
			}

			return result;
		}
		
		public bool TryGetFloat(string key, PropertyConstraints constraint, out float result, out string message,
			IReloadable target = null)
		{
			Track(key, PropertyTypes.Float, target);

			if (!float.TryParse(map[key], out result))
			{
				message = Format(key, PropertyTypes.Float);

				return false;
			}

			if (Validate(result, constraint))
			{
				message = null;

				return true;
			}
			
			message = Format(key, constraint);

			return false;
		}

		public bool TryGetFloat(string key, PropertyConstraints constraint, out string message, ref float result,
			IReloadable target = null)
		{
			if (!TryGetFloat(key, constraint, out var value, out message, target))
			{
				return false;
			}

			result = value;

			return true;
		}

		public bool TryGetFloats(string[] keys, PropertyConstraints constraint, out Dictionary<string, float> results,
			out string message, IReloadable target = null)
		{
			results = new Dictionary<string, float>();

			foreach (var key in keys)
			{
				if (!TryGetFloat(key, constraint, out var result, out message, target))
				{
					return false;
				}

				results.Add(key, result);
			}

			message = null;

			return true;
		}

		public string GetString(string key)
		{
			Track(key, PropertyTypes.String, null);

			return map[key];
		}

		public Color GetColor(string key)
		{
			Track(key, PropertyTypes.Color, null);

			if (!Color.TryParse(map[key], out var result))
			{
				Debug.Fail(Format(key, PropertyTypes.Color));
			}

			return result;
		}

		public bool TryGetColor(string key, out Color result, out string message, IReloadable target = null)
		{
			Track(key, PropertyTypes.Color, target);

			if (!Color.TryParse(map[key], out result))
			{
				message = Format(key, PropertyTypes.Color);

				return false;
			}

			// Since colors use bytes, they can't be constrained (the byte conversion would fail if any value is
			// outside the 0-255 range).
			message = null;

			return true;
		}

		public bool TryGetColors(string[] keys, out Dictionary<string, Color> results, out string message,
			IReloadable target = null)
		{
			results = new Dictionary<string, Color>();

			foreach (var key in keys)
			{
				if (!TryGetColor(key, out var result, out message, target))
				{
					return false;
				}

				results.Add(key, result);
			}

			message = null;

			return true;
		}

		private void Track(string key, PropertyTypes type, IReloadable target)
		{
			Debug.Assert(map.ContainsKey(key), $"Missing property '{key}'.");

			// This is useful for properties that should never be iterated during gameplay (or where live modification
			// would be too cumbersome or error-prone).
			if (target == null)
			{
				return;
			}

			var matchedProperty = tracker.Keys.FirstOrDefault(p => p.Key == key);

			// This means that the given target is the first to track the given key.
			if (matchedProperty.Key == null)
			{
				var list = new List<IReloadable>();
				list.Add(target);

				tracker.Add((key, type), list);
			}
			else
			{
				Debug.Assert(type == matchedProperty.Type, $"Type conflict for property '{key}' (was previously " +
					$"retrieved as {matchedProperty.Type}, but is now being retrieved as {type}).");

				var targets = tracker[matchedProperty];

				if (!targets.Contains(target))
				{
					targets.Add(target);
				}
			}
		}

		private bool Validate(int i, PropertyConstraints constraint)
		{
			switch (constraint)
			{
				case PropertyConstraints.None: return true;
				case PropertyConstraints.NonNegative: return i >= 0;
				case PropertyConstraints.Positive: return i > 0;
			}

			Debug.Fail($"Invalid integer constraint ({constraint}).");

			return false;
		}

		private bool Validate(float f, PropertyConstraints constraint)
		{
			switch (constraint)
			{
				case PropertyConstraints.None: return true;
				case PropertyConstraints.NonNegative: return f >= 0;
				case PropertyConstraints.Positive: return f > 0;
				case PropertyConstraints.ZeroToOne: return f >= 0 && f <= 1;
			}

			Debug.Fail($"Invalid float constraint ({constraint}).");

			return false;
		}

		private string Format(string key, PropertyConstraints constraint)
		{
			var tokens = key.Split('.');
			tokens[0] = tokens[0].Capitalize();

			return $"{string.Join(" ", tokens)} must be {validationStrings[(int)constraint]}.";
		}

		private string Format(string key, PropertyTypes type)
		{
			var s = type == PropertyTypes.UnsignedInteger ? "unsigned integer" : type.ToString().Uncapitalize();

			return $"Property '{key} = {map[key]}' is not a valid {s}.";
		}
	}
}

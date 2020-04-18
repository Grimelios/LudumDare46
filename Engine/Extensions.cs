using System;
using GlmSharp;
using Newtonsoft.Json.Linq;

namespace Engine
{
	public static class Extensions
	{
		public static int ToRgba(this System.Drawing.Color color)
		{
			// The Color class already has a function called "ToARGB", but that's the wrong ordering for what the
			// engine's GL code expects.
			var bytes = new []
			{
				color.R,
				color.G,
				color.B,
				color.A
			};

			return BitConverter.ToInt32(bytes, 0);
		}

		public static vec4 ToVec4(this quat q)
		{
			var values = q.Values;

			return new vec4(values[0], values[1], values[2], values[3]);
		}

		public static string StripPath(this string s)
		{
			var index = s.LastIndexOf('/');

			return s.Substring(index + 1);
		}

		public static string StripPath(this string s, out string path)
		{
			var index = s.LastIndexOf('/');

			path = s.Substring(0, index + 1);

			return s.Substring(index + 1);
		}

		public static string StripExtension(this string s)
		{
			var index = s.LastIndexOf('.');

			return s.Substring(0, index);
		}

		public static string Capitalize(this string s)
		{
			return s.Length == 0 ? s : char.ToUpper(s[0]) + s.Substring(1);
		}
		
		public static string Uncapitalize(this string s)
		{
			return s.Length == 0 ? s : char.ToLower(s[0]) + s.Substring(1);
		}

		public static bool TryParse<T>(this JToken token, string key, out T result)
		{
			var data = token[key];

			if (data != null)
			{
				result = data.Value<T>();

				return true;
			}

			result = default;

			return false;
		}

		public static bool EqualsCaseInsensitive(this char c, char other)
		{
			// See https://stackoverflow.com/a/1394898/7281613.
			return char.ToUpperInvariant(c) == char.ToUpperInvariant(other);
		}
	}
}

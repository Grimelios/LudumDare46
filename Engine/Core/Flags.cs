using System.Diagnostics;

namespace Engine.Core
{
	// This class should be constrained to enums, but that's not currently possible in C# (at least not easily).
	[DebuggerDisplay("{Value}")]
	public class Flags<T> where T : struct
	{
		public Flags(T value)
		{
			Value = value;
		}

		public T Value { get; set; }
	}
}

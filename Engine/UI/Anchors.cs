using System;

namespace Engine.UI
{
	// This is almost identical to Alignments, but excludes the Custom option (which doesn't really make sense when
	// anchoring UI elements).
	[Flags]
	public enum Anchors
	{
		Center = 0,
		Left = 1,
		Right = 2,
		Top = 4,
		Bottom = 8,
		None = 16,
	}
}

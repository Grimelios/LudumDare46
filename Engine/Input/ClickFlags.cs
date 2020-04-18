using System;

namespace Engine.Input
{
	// TODO: Support WasUnhovered and WasReleased (if needed).
	// This is useful to react to how a clickable set processed the mouse. For example, a menu might use a pointer
	// sprite that snaps to new items as they're hovered.
	[Flags]
	public enum ClickFlags
	{
		None = 0,
		WasClicked = 1,
		WasHovered = 2
	}
}

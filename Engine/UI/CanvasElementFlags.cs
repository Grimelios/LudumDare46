using System;

namespace Engine.UI
{
	[Flags]
	public enum CanvasElementFlags
	{
		None = 0,
		IsAdded = 1<<0,
		IsChild = 1<<1,
		IsDisposed = 1<<2,
		IsDrawEnabled = 1<<3,
		IsInitialized = 1<<4,
		IsRemoved = 1<<5
	}
}

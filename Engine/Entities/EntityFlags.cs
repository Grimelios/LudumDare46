using System;

namespace Engine.Entities
{
	[Flags]
	public enum EntityFlags
	{
		None = 0,

		IsAdded = 1<<0,
		IsRemoved = 1<<1,
		IsDisposed = 1<<2,
		IsInitialized = 1<<3,
		IsPositionSet = 1<<4,
		IsRotationSet = 1<<5,
		IsOrientationSet = 1<<6,

		IsUpdateEnabled = 1<<7,
		IsDrawEnabled = 1<<8,

		HandlesResolved = 1<<9
	}
}
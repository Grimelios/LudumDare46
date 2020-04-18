using System;

namespace Engine.Interfaces._3D
{
	public interface IRenderTransformable3D : IRenderPositionable3D, IRenderOrientable
	{
		// This is needed to resolve the apparent conflict between Recompute functions. Seems weird to me, but
		// thankfully easy to fix.
		new void Recompute(float t);
	}
}

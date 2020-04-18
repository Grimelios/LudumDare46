using System;

namespace Engine.Interfaces._2D
{
	public interface IRenderTransformable2D : IRenderPositionable2D, IRenderRotatable
	{
		bool IsDrawEnabled { get; set; }

		new void Recompute(float t);
	}
}

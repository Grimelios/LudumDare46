using System;

namespace Engine.Interfaces._3D
{
	public interface IRenderTargetUser3D : IDisposable
	{
		bool IsDrawEnabled { get; set; }

		void DrawTargets(float t);
	}
}

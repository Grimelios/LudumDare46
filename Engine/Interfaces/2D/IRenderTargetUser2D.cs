using System;
using Engine.Graphics._2D;

namespace Engine.Interfaces._2D
{
	public interface IRenderTargetUser2D : IDisposable
	{
		bool IsDrawEnabled { get; set; }

		void DrawTargets(SpriteBatch sb, float t);
	}
}

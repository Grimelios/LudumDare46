using System;
using System.Collections.Generic;
using Engine.Loops;
using Engine.View._2D;
using static Engine.Graphics.GL;

namespace Engine
{
	public class Game2D<TLoop, TLoopType> : Game<Camera2D, CameraView2D, TLoop, TLoopType>
		where TLoop : GameLoop2D<TLoopType>
		where TLoopType : struct
	{
		public Game2D(string title, Dictionary<TLoopType, Type> loops, TLoopType loopType, int updateTick = 60,
			int renderTick = 60) : base(title, loops, loopType, updateTick, renderTick)
		{
			activeLoop = CreateLoop(loopType);

			// For 2D games, these can be permanently disabled.
			glDisable(GL_DEPTH_TEST);
			glDisable(GL_CULL_FACE);
			glDepthFunc(GL_NEVER);
		}

		protected override void Draw(float t)
		{
			camera.Recompute(t);

			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glViewport(0, 0, (uint)Resolution.WindowWidth, (uint)Resolution.WindowHeight);

			sb.ApplyTarget(null);
			sb.Camera = camera; activeLoop.Draw(sb, t);
			sb.Flush();

			sb.Camera = null;
			canvas.Draw(sb, t);
			sb.Flush();
		}
	}
}

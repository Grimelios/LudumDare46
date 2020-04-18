using System;
using System.Collections.Generic;
using Engine.Core._2D;
using Engine.Graphics;
using Engine.Graphics._3D.Rendering;
using Engine.Loops;
using Engine.Messaging;
using Engine.View._3D;
using static Engine.Graphics.GL;

namespace Engine
{
	public abstract class Game3D<TLoop, TLoopType> : Game<Camera3D, CameraView3D, TLoop, TLoopType>
		where TLoop : GameLoop3D<TLoopType>
		where TLoopType : struct
	{
		private MasterRenderer3D renderer;
		private RenderTarget mainTarget;
		private Sprite mainSprite;

		protected Game3D(string title, Dictionary<TLoopType, Type> loops, TLoopType loopType, int updateTick = 60,
			int renderTick = 60) : base(title, loops, loopType, updateTick, renderTick)
		{
			renderer = new MasterRenderer3D(camera);
			activeLoop = CreateLoop(loopType);
			mainTarget = new RenderTarget(Resolution.RenderWidth, Resolution.RenderHeight, RenderTargetFlags.Color |
				RenderTargetFlags.Depth);
			mainSprite = new Sprite(mainTarget, Alignments.Left | Alignments.Top);
			mainSprite.Mods = SpriteModifiers.FlipVertical;

			MessageSystem.Subscribe(this, CoreMessageTypes.ResizeWindow, data =>
			{
				mainSprite.ScaleTo(Resolution.WindowWidth, Resolution.WindowHeight, false);
			});
		}

		protected override TLoop CreateLoop(TLoopType type)
		{
			var loop = CreateLoop(type, false);
			loop.Renderer = renderer;
			loop.Initialize();

			return loop;
		}

		protected override void Draw(float t)
		{
			// Render 3D targets.
			glEnable(GL_DEPTH_TEST);
			glEnable(GL_CULL_FACE);
			glDepthFunc(GL_LEQUAL);

			camera.Recompute(t);
			activeLoop.DrawTargets(t);
			mainTarget.Apply();
			activeLoop.Draw(t);

			// Render 2D targets.
			glDisable(GL_DEPTH_TEST);
			glDisable(GL_CULL_FACE);
			glDepthFunc(GL_NEVER);

			canvas.DrawTargets(sb, t);

			// Draw to the main screen.
			glBindFramebuffer(GL_FRAMEBUFFER, 0);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
			glViewport(0, 0, (uint)Resolution.WindowWidth, (uint)Resolution.WindowHeight);

			sb.ApplyTarget(null);
			mainSprite.Draw(sb, t);
			canvas.Draw(sb, t);
			sb.Flush();
		}
	}
}

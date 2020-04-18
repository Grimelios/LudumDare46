using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Core;
using Engine.Graphics;
using Engine.Graphics._2D;
using Engine.Input;
using Engine.Loops;
using Engine.Messaging;
using Engine.UI;
using Engine.View;
using GlmSharp;
using static Engine.Graphics.GL;
using static Engine.GLFW;

namespace Engine
{
	// TODO: Pull monitor data on launch.
	// TODO: Consider responding to monitors being disconnected (might not be worth the trouble).
	public abstract class Game<TCamera, TView, TLoop, TLoopType> : IReceiver
		where TCamera : Camera<TCamera, TView>, new()
		where TView : CameraView<TCamera, TView>
		where TLoop : GameLoop<TLoopType, TCamera, TView>
		where TLoopType : struct
	{
		// The input processor (and associated callbacks) are static to avoid the garbage collector moving things
		// around at runtime (which would cause an error in GLFW).
		private static InputProcessor inputProcessor;
		private static GLFWkeyfun keyCallback;
		private static GLFWcursorposfun cursorPositionCallback;
		private static GLFWmousebuttonfun mouseButtonCallback;
		private static GLFWwindowfocusfun focusCallback;

		private static float dtUpdate;
		private static float dtRender;

		static Game()
		{
			keyCallback = KeyCallback;
			cursorPositionCallback = CursorPositionCallback;
			mouseButtonCallback = MouseButtonCallback;
			focusCallback = FocusCallbackInternal;
		}

		protected static Action<bool> FocusCallback { get; set; }

		private static void KeyCallback(IntPtr window, int key, int scancode, int action, int mods)
		{
			// Key can be negative in some cases (like Alt + Printscreen).
			if (key == -1)
			{
				return;
			}

			switch (action)
			{
				case GLFW_PRESS: inputProcessor.OnKeyPress(key, mods); break;
				case GLFW_RELEASE: inputProcessor.OnKeyRelease(key); break;
			}
		}

		private static void CursorPositionCallback(IntPtr window, double x, double y)
		{
			// TODO: If high-DPI mouse coordinates are enabled, these shouldn't be cast to integers.
			// TODO: When focus is lost and regained, add code to prevent an artifically huge delta.
			var mX = (int)Math.Round(x);
			var mY = (int)Math.Round(y);

			inputProcessor.OnMouseMove(mX, mY);
		}

		private static void MouseButtonCallback(IntPtr window, int button, int action, int mods)
		{
			switch (action)
			{
				case GLFW_PRESS: inputProcessor.OnMouseButtonPress(button); break;
				case GLFW_RELEASE: inputProcessor.OnMouseButtonRelease(button); break;
			}
		}

		private static void FocusCallbackInternal(IntPtr window, int isFocused)
		{
			FocusCallback?.Invoke(isFocused != 0);
		}

		private float previousTime;
		private float updateAccumulator;
		private float renderAccumulator;

		protected Canvas canvas;
		protected SpriteBatch sb;
		protected Window window;

		protected TCamera camera;
		protected TLoop activeLoop;

		private Dictionary<TLoopType, Type> loops;
		private List<CanvasElement> pauseElements;

		private TLoopType currentLoopType;
		private TLoopType nextLoopType;

		private bool isPaused;
		private bool justPaused;

		// Using a wrapper allows other classes (like the terminal) to toggle pause without requiring access to the
		// entire class.
		private Primitive<bool> isPauseToggled;

		// When the game is paused, the current interpolation value needs to be stored (to prevent visible jitter while
		// paused).
		private float tPaused;

		protected Game(string title, Dictionary<TLoopType, Type> loops, TLoopType loopType, int updateTick = 60,
			int renderTick = 60)
		{
			Debug.Assert(updateTick > 0, "Update tick must be positive.");
			Debug.Assert(renderTick > 0, "Render tick must be positive.");
			Debug.Assert(loops.ContainsKey(loopType), $"Can't launch into loop type '{loopType}' (no associated " +
				"class provided).");
			Debug.Assert(loops.Values.All(t => typeof(TLoop).IsAssignableFrom(t)), "Invalid type in loop map (all " +
				$"types must extend {typeof(TLoop).FullName}).");

			this.loops = loops;

			dtUpdate = 1f / updateTick;
			dtRender = 1f / renderTick;
			RenderTick = renderTick;

			DeltaTime.Value = dtUpdate;

			glfwInit();
			glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
			glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 4);
			glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

			var address = glfwCreateWindow(Resolution.WindowWidth, Resolution.WindowHeight, title, IntPtr.Zero,
				IntPtr.Zero);

			if (address == IntPtr.Zero)
			{
				glfwTerminate();

				return;
			}

			window = new Window(Resolution.WindowWidth, Resolution.WindowHeight, address);
			inputProcessor = InputProcessor.Instance;

			// TODO: Is glfwSetInputMode(window, GLFW_LOCK_KEY_MODS, GLFW_TRUE) needed? (see the GLFW input guide)
			glfwMakeContextCurrent(address);
			glfwSetKeyCallback(address, keyCallback);
			glfwSetCursorPosCallback(address, cursorPositionCallback);
			glfwSetMouseButtonCallback(address, mouseButtonCallback);
			glfwSetWindowFocusCallback(address, focusCallback);

			glClearColor(0, 0, 0, 1);
			glEnable(GL_BLEND);
			glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
			glPrimitiveRestartIndex(Constants.PrimitiveRestartIndex);

			camera = new TCamera();
			canvas = new Canvas();
			sb = new SpriteBatch();
			isPauseToggled = new Primitive<bool>();

			// The loop itself is created via a call to CreateLoop from parent constructors.
			currentLoopType = loopType;
			nextLoopType = loopType;

			MessageSystem.Subscribe(this, CoreMessageTypes.Exit, data =>
			{
				glfwSetWindowShouldClose(window.Address, 1);
			});

			MessageSystem.Subscribe(this, CoreMessageTypes.Gamestate, data =>
			{
				nextLoopType = (TLoopType)data;
			});

			MessageSystem.Send(CoreMessageTypes.ResizeRender, Resolution.RenderDimensions);
			MessageSystem.Send(CoreMessageTypes.ResizeWindow, Resolution.WindowDimensions);
		}

		public List<MessageHandle> MessageHandles { get; set; }

		// While the game is paused, the entire game is effectively frozen (including most UI elements), *except* for
		// elements in this list. Similarly, for rendering, interpolation is frozen (except for this list).
		public List<CanvasElement> PauseElements => pauseElements ??= new List<CanvasElement>();

		public ivec2 Location
		{
			set => glfwSetWindowPos(window.Address, value.x, value.y);
		}

		public bool IsCursorEnabled
		{
			set => glfwSetInputMode(window.Address, GLFW_CURSOR, value ? GLFW_CURSOR_NORMAL : GLFW_CURSOR_DISABLED);
		}

		// Update tick is intentionally not exposed (in order to force consistency among machines, especially with
		// regard to physics).
		public int RenderTick { get; set; }

		protected virtual TLoop CreateLoop(TLoopType type)
		{
			return CreateLoop(type, true);
		}

		// Bit of a hack here (the two overloaded functions) in order to accommodate 3D games, which need to assign the
		// renderer before the loop is initialized.
		protected TLoop CreateLoop(TLoopType type, bool shouldInitialize)
		{
			var loop = (TLoop)Activator.CreateInstance(loops[type]);
			loop.Camera = camera;
			loop.Canvas = canvas;
			loop.SpriteBatch = sb;

			if (shouldInitialize)
			{
				loop.Initialize();
			}

			return loop;
		}

		public void TogglePause()
		{
			isPauseToggled = true;
		}

		public void Run()
		{
			while (glfwWindowShouldClose(window.Address) == 0)
			{
				var time = (float)glfwGetTime();
				var delta = time - previousTime;

				updateAccumulator += delta;
				renderAccumulator += delta;
				previousTime = time;
				
				// TODO: Consider using a while loop (with max iterations) to avoid slowdown on weaker machines.
				if (updateAccumulator >= dtUpdate)
				{
					glfwPollEvents();
					inputProcessor.Refresh();
					Update();

					// TODO: If a while loop is used, this should revert back to -= (rather than modulus).
					updateAccumulator %= dtUpdate;
				}
				
				if (renderAccumulator >= dtRender)
				{
					// TODO: If a while loop is used above, the t value (passed to Draw) will need to use modulus.
					DrawInternal(updateAccumulator / dtUpdate);
					glfwSwapBuffers(window.Address);
					renderAccumulator %= dtRender;
				}
			}

			Dispose();
			glfwTerminate();
		}

		public virtual void Dispose()
		{
		}

		private void Update()
		{
			if (isPaused)
			{
				PauseElements.ForEach(e => e.Update());
			}
			else
			{
				activeLoop.Update();
				canvas.Update();
			}

			// Pausing incurs an intentional one-frame buffer. In other words, on the frame the game is paused, the
			// game still updates one additional time. Similarly, on the frame the game is unpaused, the game doesn't
			// begin updating again until the *next* frame.
			if (isPauseToggled)
			{
				isPaused = !isPaused;
				isPauseToggled = false;
				justPaused = isPaused;
			}

			if (!nextLoopType.Equals(currentLoopType))
			{
				activeLoop.Dispose();
				activeLoop = CreateLoop(nextLoopType);
				currentLoopType = nextLoopType;
			}
		}

		private void DrawInternal(float t)
		{
			if (justPaused)
			{
				tPaused = t;
				justPaused = false;
			}

			Draw(isPaused ? tPaused : t);

			if (isPaused)
			{
				// While paused, pause-related UI elements (like the pause menu) should still be interpolated
				// properly (rather than frozen like everything else).
				PauseElements.ForEach(e => e.Draw(sb, t));
				sb.Flush();
			}
		}

		protected abstract void Draw(float t);
	}
}

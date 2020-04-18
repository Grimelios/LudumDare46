using System;
using System.Diagnostics;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.UI;
using Engine.View;

namespace Engine.Loops
{
	public abstract class GameLoop<TType, TCamera, TView> : IDynamic, IDisposable
		where TType : struct
		where TCamera : Camera<TCamera, TView>
		where TView : CameraView<TCamera, TView>
	{
		static GameLoop()
		{
			var type = typeof(TType);

			Debug.Assert(type.IsEnum, $"Invalid loop type enumeration {type.FullName}.");
		}

		protected GameLoop(TType type)
		{
			Type = type;
			IsUpdateEnabled = true;
			IsDrawEnabled = true;
		}

		public TType Type { get; }
		public TCamera Camera { get; internal set; }

		public bool IsUpdateEnabled { get; set; }
		public bool IsDrawEnabled { get; set; }

		public Canvas Canvas { get; internal set; }
		public SpriteBatch SpriteBatch { get; internal set; }

		public abstract void Initialize();

		public virtual void Update()
		{
			Camera.Update();
		}

		public virtual void Dispose()
		{
			Canvas.Clear();
		}
	}
}

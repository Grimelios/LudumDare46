using Engine.Interfaces;
using Engine.Utility;

namespace Engine.Components
{
	public abstract class Motor<T> : IComponent where T : class
	{
		private T parent;
		private float rotation;

		protected Motor(T parent, bool isRunning)
		{
			this.parent = parent;

			IsRunning = isRunning;
		}

		// Motors are designed to be persistent.
		public bool IsComplete => false;
		public bool IsEnabled => IsRunning;
		public bool IsRunning { get; set; }

		public float AngularVelocity { get; set; }

		public void Reset()
		{
			rotation = 0;
			IsRunning = false;
		}

		protected abstract void Refresh(T parent, float rotation);

		public void Update()
		{
			if (IsRunning)
			{
				rotation = Utilities.RestrictAngle(rotation + AngularVelocity * DeltaTime.Value);
				Refresh(parent, rotation);
			}
		}
	}
}

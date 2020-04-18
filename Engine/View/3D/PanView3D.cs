using Engine.Timing;
using GlmSharp;

namespace Engine.View._3D
{
	public class PanView3D : CameraView3D
	{
		private SingleTimer timer;

		public PanView3D()
		{
			timer = new SingleTimer();
		}

		// TODO: Consider adding positionable targets (such that the view would automatically track the object as it moves).
		// If a target is set, the camera orients itself to face the target each frame.
		public vec3? Target { private get; set; }

		// Using this view, the camera can pan either 1) between two points (using this function), or 2) along a curved
		// path (using the overloaded version below).
		public void Refresh(vec3 p1, vec3 p2, float duration, EaseTypes ease)
		{
			if (!timer.IsPaused)
			{
				timer.Elapsed = 0;
			}

			timer.Duration = duration;
			timer.Tick = t =>
			{
				Recompute(vec3.Lerp(p1, p2, Ease.Compute(t, ease)), true);
			};

			timer.IsPaused = false;

			// When a pan begins, the camera is immediately snapped to the starting transform.
			Recompute(p1, false);
		}

		private void Recompute(vec3 p, bool shouldInterpolate)
		{
			Camera.Position.SetValue(p, true);

			if (Target.HasValue)
			{
				Camera.Orientation.SetValue(mat4.LookAt(p, Target.Value, vec3.UnitY).ToQuaternion, shouldInterpolate);
			}
		}

		public override void Update()
		{
			timer.Update();
		}
	}
}

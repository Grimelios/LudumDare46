using Engine.Interfaces;

namespace Engine.Animation
{
	public class AnimationPlayer : IComponent
	{
		private Skeleton skeleton;
		private Animation animation;

		private float elapsed;

		public AnimationPlayer(Skeleton skeleton)
		{
			this.skeleton = skeleton;
		}

		public bool IsComplete { get; private set; }
		public bool IsEnabled { get; set; }

		public void Play(Animation animation)
		{
			this.animation = animation;

			elapsed = 0;
		}

		public void Reset()
		{
			elapsed = 0;
			IsEnabled = false;
		}

		public void Update()
		{
			elapsed += DeltaTime.Value;

			var bones = skeleton.Bones;
		}
	}
}

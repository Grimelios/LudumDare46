using Engine.Interfaces._2D;

namespace Engine.Components
{
	public class Motor2D : Motor<IRotationContainer>
	{
		public Motor2D(IRotationContainer parent, bool isRunning = true) : base(parent, isRunning)
		{
		}

		protected override void Refresh(IRotationContainer parent, float rotation)
		{
			parent.SetRotation(rotation, true);
		}
	}
}

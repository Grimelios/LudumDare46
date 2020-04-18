using Engine.Interfaces._2D;
using Engine.Physics._2D;
using Engine.Sensors._2D;
using Engine.Shapes._2D;
using GlmSharp;

namespace Engine.Entities._2D
{
	public abstract class Entity2D<TGroup> : Entity<Entity2D<TGroup>, EntityHandle2D<TGroup>, TGroup, Scene2D<TGroup>,
		World2D, Sensor2D, Shape2D, ShapeTypes2D, Space2D, vec2>, IRotationContainer where TGroup : struct
	{
		private float rotation;

		protected Entity2D(TGroup group) : base(group)
		{
		}

		public float Rotation => rotation;

		public void SetRotation(float rotation, bool shouldInterpolate)
		{
			this.rotation = rotation;

			flags |= EntityFlags.IsRotationSet;
		}
	}
}

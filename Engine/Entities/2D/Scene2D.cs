using Engine.Physics._2D;
using Engine.Sensors._2D;
using Engine.Shapes._2D;
using Engine.UI;
using GlmSharp;

namespace Engine.Entities._2D
{
	public abstract class Scene2D<TGroup> : Scene<Entity2D<TGroup>, EntityHandle2D<TGroup>, TGroup, Scene2D<TGroup>,
		World2D, Sensor2D, Shape2D, ShapeTypes2D, Space2D, vec2> where TGroup : struct
	{
		protected Scene2D(Canvas canvas, World2D world, Space2D space) : base(canvas, world, space)
		{
		}
	}
}

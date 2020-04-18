using System.Diagnostics;
using Engine.Interfaces._2D;
using Engine.Shapes._2D;
using GlmSharp;

namespace Engine.Sensors._2D
{
	public class Sensor2D : Sensor<Sensor2D, Shape2D, ShapeTypes2D, Space2D, vec2>, IPositionable2D, IRotatable
	{
		private float rotation;

		public Sensor2D(SensorTypes type, object owner, Shape2D shape, uint groups, uint affects) :
			base(type, owner, shape, groups, affects, false)
		{
		}

		public virtual float Rotation
		{
			get => rotation;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				rotation = value;

				if (Shape != null)
				{
					Shape.Rotation = value;
				}
			}
		}

		public override Shape2D Shape
		{
			get => base.Shape;
			set
			{
				base.Shape = value;

				if (value != null)
				{
					value.Rotation = rotation;
				}
			}
		}
	}
}

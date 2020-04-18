using System.Diagnostics;
using Engine.Interfaces._3D;
using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Sensors._3D
{
	public class Sensor3D : Sensor<Sensor3D, Shape3D, ShapeTypes3D, Space3D, vec3>, ITransformable3D
	{
		private quat orientation;

		public Sensor3D(SensorTypes type, object owner, Shape3D shape, uint groups, uint affects) :
			base(type, owner, shape, groups, affects, false)
		{
		}

		public virtual quat Orientation
		{
			get => orientation;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				orientation = value;

				if (Shape != null)
				{
					Shape.Orientation = value;
				}
			}
		}

		public override Shape3D Shape
		{
			get => base.Shape;
			set
			{
				base.Shape = value;

				if (value != null)
				{
					value.Orientation = orientation;
				}
			}
		}

		public virtual void SetTransform(vec3 p, quat q)
		{
			Position = p;
			orientation = q;
		}
	}
}

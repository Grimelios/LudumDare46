using System.Collections.Generic;
using System.Linq;
using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Sensors._3D
{
	public class MultiSensor3D : Sensor3D
	{
		private List<(Shape3D Shape, vec3 Position, quat Orientation)> attachments;

		public MultiSensor3D(SensorTypes type, object owner, uint groups, uint affects) :
			base(type, owner, null, groups, affects)
		{
			attachments = new List<(Shape3D Shape, vec3 Position, quat Orientation)>();
		}

		public override vec3 Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				RecomputeAttachments();
			}
		}

		public override quat Orientation
		{
			get => base.Orientation;
			set
			{
				base.Orientation = value;

				RecomputeAttachments();
			}
		}

		private void RecomputeAttachments()
		{
			foreach (var (shape, p, q) in attachments)
			{
				shape.Position = Position + Orientation * p;
				shape.Orientation = Orientation * q;
			}
		}

		public void Attach(Shape3D shape, vec3 p, quat q)
		{
			attachments.Add((shape, p, q));
		}

		public override bool Overlaps(Sensor3D other)
		{
			if (other.IsCompound)
			{
				return attachments.Any(a => a.Shape.Overlaps(other.Shape));
			}

			var multi = (MultiSensor3D)other;

			foreach (var a1 in attachments)
			{
				var shape = a1.Shape;

				if (multi.attachments.Any(a2 => shape.Overlaps(a2.Shape)))
				{
					return true;
				}
			}

			return false;
		}
	}
}

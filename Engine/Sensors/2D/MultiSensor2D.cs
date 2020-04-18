using System.Collections.Generic;
using System.Linq;
using Engine.Shapes._2D;
using Engine.Utility;
using GlmSharp;

namespace Engine.Sensors._2D
{
	public class MultiSensor2D : Sensor2D
	{
		private List<(Shape2D Shape, vec2 Position, float Rotation)> attachments;

		public MultiSensor2D(SensorTypes type, object owner, uint groups, uint affects) :
			base(type, owner, null, groups, affects)
		{
			attachments = new List<(Shape2D Shape, vec2 Position, float Rotation)>();
		}

		public override vec2 Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				RecomputeAttachments();
			}
		}

		public override float Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;

				RecomputeAttachments();
			}
		}

		private void RecomputeAttachments()
		{
			foreach (var (shape, p, r) in attachments)
			{
				shape.Rotation = Rotation + r;
				shape.Position = Position + Utilities.Rotate(p, r);
			}
		}

		public void Attach(Shape2D shape, vec2 p, float r)
		{
			attachments.Add((shape, p, r));
		}

		public override bool Overlaps(Sensor2D other)
		{
			if (other.IsCompound)
			{
				return attachments.Any(a => a.Shape.Overlaps(other.Shape));
			}

			var multi = (MultiSensor2D)other;

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

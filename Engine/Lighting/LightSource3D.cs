using System.Diagnostics;
using Engine.Core;
using Engine.Core._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;

namespace Engine.Lighting
{
	public abstract class LightSource3D : IRenderTransformable3D, IRenderColorable
	{
		private RenderPosition3DField position;
		private RenderOrientationField orientation;
		private RenderColorField color;

		private float range;

		protected LightSource3D()
		{
			var flags = new Flags<TransformFlags>(TransformFlags.None);

			position = new RenderPosition3DField(flags);
			orientation = new RenderOrientationField(flags);
			color = new RenderColorField(flags);
		}

		public RenderPosition3DField Position => position;
		public RenderOrientationField Orientation => orientation;
		public RenderColorField Color => color;

		public float Range
		{
			get => range;
			set
			{
				Debug.Assert(range >= 0, "Light source range can't be negative.");
				
				range = value;
			}
		}
		
		public void Recompute(float t)
		{
			position.Evaluate(t);
			orientation.Evaluate(t);
			color.Evaluate(t);
		}
	}
}

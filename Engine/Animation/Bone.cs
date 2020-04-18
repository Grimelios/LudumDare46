using Engine.Core;
using Engine.Core._3D;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Animation
{
	// TODO: Interpolate transforms (at time of writing, I'm not sure whether local or absolute positions need to be interpolated, or both).
	public class Bone : IRenderTransformable3D
	{
		private RenderPosition3DField position;
		private RenderOrientationField orientation;

		public Bone()
		{
			var flags = new Flags<TransformFlags>(TransformFlags.None);

			position = new RenderPosition3DField(flags);
			orientation = new RenderOrientationField(flags);
		}

		public bool IsDrawEnabled { get; set; }

		public RenderPosition3DField Position => position;
		public RenderOrientationField Orientation => orientation;

		/*
		public vec3 LocalPosition { get; set; }
		public quat LocalOrientation { get; set; }
		public Bone[] Children { get; }
		*/

		public mat4 Matrix { get; private set; }

		public void Recompute(float t)
		{
			var p = position.Evaluate(t);
			var q = orientation.Evaluate(t);

			Matrix = mat4.Translate(p) * q.ToMat4;
		}
	}
}
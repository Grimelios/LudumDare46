using Engine.Core;
using Engine.Core._2D;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.View._2D
{
	public class Camera2D : Camera<Camera2D, CameraView2D>, IRenderPositionable2D, IRenderRotatable
	{
		private RenderPosition2DField position;
		private RenderRotationField rotation;
		private RenderScale2DField zoom;

		public Camera2D()
		{
			var flags = new Flags<TransformFlags>(TransformFlags.None);

			position = new RenderPosition2DField(flags);
			rotation = new RenderRotationField(flags);
			zoom = new RenderScale2DField(flags);
			Origin = Resolution.WindowDimensions / 2;
		}

		public vec2 Origin { get; set; }

		public RenderPosition2DField Position => position;
		public RenderRotationField Rotation => rotation;
		public RenderScale2DField Zoom => zoom;

		public mat4 Matrix { get; private set; }
		public mat4 MatrixInverse { get; private set; }

		public vec2 ToWorld(vec2 p)
		{
			return (Matrix * new vec4(p.x, p.y, 0, 1)).swizzle.xy;
		}

		public vec2 ToScreen(vec2 p)
		{
			return (MatrixInverse * new vec4(p.x, p.y, 0, 1)).swizzle.xy;
		}

		public void Recompute(float t)
		{
			var p = position.Evaluate(t);
			var r = rotation.Evaluate(t);
			var z = zoom.Evaluate(t);
			z.x = z.x == 0 ? 0 : 1 / z.x;
			z.y = z.y == 0 ? 0 : 1 / z.y;

			Matrix = mat4.Translate(p.x, p.y, 0) * mat4.Scale(z.x, z.y, 1) * mat4.RotateZ(r) * mat4.Translate(-Origin.x, -Origin.y, 0);
			MatrixInverse = Matrix.Inverse;
		}
	}
}

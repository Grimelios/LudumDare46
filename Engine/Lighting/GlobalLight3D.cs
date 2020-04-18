using System.Diagnostics;
using Engine.Core;
using Engine.Interfaces;
using Engine.Utility;
using Engine.View._3D;
using GlmSharp;

namespace Engine.Lighting
{
	public class GlobalLight3D : IRenderColorable
	{
		private RenderColorField color;

		private vec3 direction;

		private float ambientIntensity;
		private float shadowNearPlane;
		private float shadowFarPlane;

		public GlobalLight3D()
		{
			color = new RenderColorField(new Flags<TransformFlags>(TransformFlags.None));
			ambientIntensity = 0.1f;
			shadowNearPlane = 0.1f;
			shadowFarPlane = 100;
		}

		public RenderColorField Color => color;

		public float AmbientIntensity
		{
			get => ambientIntensity;
			set
			{
				Debug.Assert(value >= 0 && value <= 1, "Ambient intensity must be between zero and one (inclusive).");

				ambientIntensity = value;
			}
		}

		public vec3 Direction
		{
			get => direction;
			set
			{
				Debug.Assert(Utilities.LengthSquared(value) > 0, "Global light direction can't have zero length.");

				direction = value;
			}
		}

		public mat4 Matrix { get; private set; }
		public mat4 BiasMatrix { get; private set; }

		public void SetShadowPlanes(float near, float far)
		{
			Debug.Assert(near > 0, "Shadow near plane must be positive.");
			Debug.Assert(far > 0, "Shadow far plane must be positive.");
			Debug.Assert(far > near, "Shadow far plane must be larger than the near plane.");

			shadowNearPlane = near;
			shadowFarPlane = far;
		}

		public void RecomputeMatrices(Camera3D camera)
		{
			// See http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-16-shadow-mapping/.
			var bias = new mat4
			(
				0.5f, 0.0f, 0.0f, 0,
				0.0f, 0.5f, 0.0f, 0,
				0.0f, 0.0f, 0.5f, 0,
				0.5f, 0.5f, 0.5f, 1
			);

			// TODO: Compute the shadow box based on camera view.
			var orthoHalfSize = new vec3(10, 10, 20);
			var p = camera.Position.ResultValue;

			// The light matrix is positioned such that the far plane exactly hits the back side of the camera's view
			// box (from the light's perspective). This gives as much room as possible for off-screen objects to still
			// cast visible shadows.
			var lightView = mat4.LookAt(p - Direction * (shadowFarPlane - shadowNearPlane -orthoHalfSize.z), p,
				vec3.UnitY);
			var lightProjection = mat4.Ortho(-orthoHalfSize.x, orthoHalfSize.x, -orthoHalfSize.y, orthoHalfSize.y,
				shadowNearPlane, shadowFarPlane);

			Matrix = lightProjection * lightView;
			BiasMatrix = bias * Matrix;
		}

		public void Recompute(float t)
		{
			color.Evaluate(t);
		}
	}
}

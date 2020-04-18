using System.Diagnostics;
using Engine.Core;
using Engine.Core._3D;
using Engine.Interfaces._3D;
using Engine.Messaging;
using Engine.Utility;
using GlmSharp;

namespace Engine.View._3D
{
	// TODO: Does FOV need to be interpolated? (maybe for some visual effects?)
	public class Camera3D : Camera<Camera3D, CameraView3D>, IRenderPositionable3D, IRenderOrientable
	{
		private RenderPosition3DField position;
		private RenderOrientationField orientation;

		private mat4 projection;

		private float orthoHalfWidth;
		private float orthoHalfHeight;
		private float nearPlane;
		private float farPlane;
		private float fov;

		private bool isOrthographic;

		public Camera3D()
		{
			var flags = new Flags<TransformFlags>(TransformFlags.None);

			position = new RenderPosition3DField(flags);
			orientation = new RenderOrientationField(flags);

			OrthoWidth = 16;
			OrthoHeight = 9;
			NearPlane = 0.1f;
			FarPlane = 1000;
			fov = Constants.PiOverTwo;

			MessageSystem.Subscribe(this, (int)CoreMessageTypes.ResizeRender, data =>
			{
				RecomputeProjection();
			});
		}

		public float OrthoWidth
		{
			get => orthoHalfWidth * 2;
			set
			{
				Debug.Assert(value > 0, "Orthographic width must be positive.");

				orthoHalfWidth = value / 2;
			}
		}

		public float OrthoHeight
		{
			get => orthoHalfHeight * 2;
			set
			{
				Debug.Assert(value > 0, "Orthographic height must be positive.");

				orthoHalfHeight = value / 2;
			}
		}

		public float NearPlane
		{
			get => nearPlane;
			set
			{
				Debug.Assert(value > 0, "Near plane must be positive.");

				nearPlane = value;
			}
		}

		public float FarPlane
		{
			get => farPlane;
			set
			{
				Debug.Assert(value > 0, "Far plane must be positive.");

				farPlane = value;
			}
		}

		public float Fov => fov;

		public bool IsOrthographic
		{
			get => isOrthographic;
			set
			{
				isOrthographic = value;
				RecomputeProjection();
			}
		}

		public RenderPosition3DField Position => position;
		public RenderOrientationField Orientation => orientation;

		public mat4 ViewProjection { get; private set; }
		public mat4 ViewProjectionInverse { get; private set; }

		public void SetFov(float fov, AngleUnits units)
		{
			Debug.Assert(fov > 0, "Field of view must be positive.");

			this.fov = units == AngleUnits.Degrees ? Utilities.ToRadians(fov) : fov;

			RecomputeProjection();
		}

		private void RecomputeProjection()
		{
			var dimensions = Resolution.RenderDimensions;

			projection = isOrthographic
				? mat4.Ortho(-orthoHalfWidth, orthoHalfWidth, -orthoHalfHeight, orthoHalfHeight, NearPlane, FarPlane)
				: mat4.PerspectiveFov(fov, dimensions.x, dimensions.y, NearPlane, FarPlane);
		}

		public void Recompute(float t)
		{
			var p = position.Evaluate(t);
			var q = orientation.Evaluate(t);
			var view = new mat4(q) * mat4.Translate(-p.x, -p.y, -p.z);

			ViewProjection = projection * view;
			ViewProjectionInverse = ViewProjection.Inverse;
		}

		public vec3 ToScreen(vec3 p)
		{
			// TODO: Might be able to optimize by not converting to a quaternion first.
			return ViewProjectionInverse.ToQuaternion * p;
		}
	}
}

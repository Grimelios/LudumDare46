using Engine.Graphics._3D;
using Engine.Interfaces._3D;
using Engine.Shaders;
using GlmSharp;

namespace Engine.Core._3D
{
	public abstract class MeshUser : IRenderable3D, IRenderScalable3D
	{
		private RenderPosition3DField position;
		private RenderScale3DField scale;
		private RenderOrientationField orientation;

		protected MeshUser(Mesh mesh)
		{
			Mesh = mesh;
			IsDrawEnabled = true;
			IsShadowCaster = true;

			var flags = new Flags<TransformFlags>(TransformFlags.None);

			// The corresponding properties are intentionally not called (in order to keep flags unset).
			position = new RenderPosition3DField(flags);
			scale = new RenderScale3DField(flags);
			orientation = new RenderOrientationField(flags);
		}

		public Mesh Mesh { get; }
		public RenderPosition3DField Position => position;
		public RenderScale3DField Scale => scale;
		public RenderOrientationField Orientation => orientation;

		public mat4 WorldMatrix { get; private set; }

		// Using custom shaders is optional.
		public Shader Shader { get; set; }
		public Shader ShadowShader { get; set; }

		public bool IsDrawEnabled { get; set; }
		public bool IsShadowCaster { get; set; }

		public virtual void Recompute(float t)
		{
			var p = position.Evaluate(t);
			var s = scale.Evaluate(t);
			var q = orientation.Evaluate(t);

			WorldMatrix = mat4.Translate(p) * q.ToMat4 * mat4.Scale(s);
		}

		// TODO: Do meshes need to be unloaded? (from the content cache and/or GPU memory)
		public void Dispose()
		{
			Shader?.Dispose();
			ShadowShader?.Dispose();
		}
	}
}

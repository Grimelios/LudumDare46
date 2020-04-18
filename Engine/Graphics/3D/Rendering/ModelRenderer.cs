using Engine.Core._3D;
using Engine.Lighting;
using Engine.Shaders;
using Engine.View._3D;
using static Engine.Graphics.GL;

namespace Engine.Graphics._3D.Rendering
{
	public class ModelRenderer : MeshRenderer<Model>
	{
		public ModelRenderer(Camera3D camera, GlobalLight3D light, uint bufferCapacity = 0, uint indexCapacity = 0) :
			base(camera, light, bufferCapacity, indexCapacity)
		{
			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "ModelShadow.vert");
			shader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(1, GL_FLOAT);
			shader.AddAttribute<float>(3, GL_FLOAT);

			Bind(bufferId, indexId);
		}

		protected override float[] GetData(Mesh mesh)
		{
			var points = mesh.Points;
			var sources = mesh.Sources;
			var normals = mesh.Normals;
			var vertices = mesh.Vertices;
			var buffer = new float[vertices.Length * 9];

			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				var p = points[v.x];
				var s = sources[v.y];
				var n = normals[v.w];
				var start = i * 9;

				buffer[start] = p.x;
				buffer[start + 1] = p.y;
				buffer[start + 2] = p.z;
				buffer[start + 3] = s.x;
				buffer[start + 4] = s.y;
				buffer[start + 5] = v.z;
				buffer[start + 6] = n.x;
				buffer[start + 7] = n.y;
				buffer[start + 8] = n.z;
			}

			return buffer;
		}
	}
}

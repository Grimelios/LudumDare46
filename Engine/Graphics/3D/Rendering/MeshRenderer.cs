using System.Collections.Generic;
using System.Linq;
using Engine.Core._3D;
using Engine.Lighting;
using Engine.View._3D;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics._3D.Rendering
{
	public abstract class MeshRenderer<T> : AbstractRenderer3D<Mesh, T> where T : MeshUser
	{
		private PrimitiveBuffer buffer;

		protected MeshRenderer(Camera3D camera, GlobalLight3D light, uint bufferCapacity = 0, uint indexCapacity = 0) :
			base(camera, light)
		{
			buffer = new PrimitiveBuffer(BufferFrequencies.Static, BufferUsages.Draw, bufferCapacity, indexCapacity);
		}

		public override unsafe void Add(T item)
		{
			var mesh = item.Mesh;

			Add(mesh, item);

			// Each mesh only needs to be buffered to GPU memory once (the first time it's used).
			if (mesh.Handle != null)
			{
				return;
			}

			var data = GetData(mesh);
			var indices = mesh.Indices;
			var handle = new MeshHandle(indices.Length, (int)buffer.IndexSize, buffer.BaseVertex);

			buffer.Buffer(data, indices);

			mesh.Handle = handle;
		}

		protected abstract float[] GetData(Mesh mesh);

		public override void Remove(T item)
		{
			Remove(item.Mesh, item);
		}

		public void Remove(IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				Remove(item);
			}
		}

		protected override void Apply(Mesh key)
		{
			var textures = key.Textures;

			for (int i = 0; i < textures.Length; i++)
			{
				textures[i].Bind((uint)i);
			}

			var materials = key.Materials;

			if (materials != null)
			{
				var specular = key.Materials.Select(m => m.Specular).ToArray();

				shader.Use();
				shader.SetUniform("materials", specular);
			}
		}

		public override void PrepareShadow()
		{
			glEnable(GL_CULL_FACE);
			glCullFace(GL_FRONT);

			base.PrepareShadow();
		}

		public override void Prepare()
		{
			// Note that face culling will already be enabled here (via PrepareShadow).
			glCullFace(GL_BACK);

			shader.Use();
			shader.SetUniform("eye", Camera.Position.ResultValue);

			base.Prepare();
		}

		public override unsafe void Draw(T item, mat4? vp)
		{
			if (vp != null)
			{
				PrepareShader(item, vp.Value);
			}

			if (buffer.BufferSize > 0)
			{
				buffer.Flush();
			}

			var handle = item.Mesh.Handle;

			glDrawElementsBaseVertex(GL_TRIANGLES, (uint)handle.Count, GL_UNSIGNED_SHORT, (void*)handle.Offset,
				handle.BaseVertex);
		}
	}
}

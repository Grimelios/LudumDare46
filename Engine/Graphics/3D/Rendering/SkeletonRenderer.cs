using System;
using System.Linq;
using Engine.Animation;
using Engine.Lighting;
using Engine.Shaders;
using Engine.View;
using Engine.View._3D;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics._3D.Rendering
{
	// TODO: Lighting is wrong on rendering certain mesh faces for skeletons (the normal might be the culprit).
	public class SkeletonRenderer : MeshRenderer<Skeleton>
	{
		private Shader shadowShader;

		public SkeletonRenderer(Camera3D camera, GlobalLight3D light, uint bufferCapacity, uint indexCapacity) :
			base(camera, light, bufferCapacity, indexCapacity)
		{
			shader = new Shader();
			shader.Attach(ShaderTypes.Vertex, "Skeletal.vert");
			shader.Attach(ShaderTypes.Fragment, "ModelShadow.frag");
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<float>(1, GL_FLOAT);
			shader.AddAttribute<float>(3, GL_FLOAT);
			shader.AddAttribute<float>(2, GL_FLOAT);
			shader.AddAttribute<ushort>(2, GL_UNSIGNED_SHORT);

			Bind(bufferId, indexId);

			shadowShader = new Shader();
			shadowShader.Attach(ShaderTypes.Vertex, "ShadowMapSkeletal.vert");
			shadowShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowShader.Initialize();
			shadowShader.Use();
			shadowShader.SetUniform("images", new[] { 0, 1, 2, 3, 4, 5 });
		}

		public override Shader ShadowShader => shadowShader;

		public override void Dispose()
		{
			shadowShader.Dispose();

			base.Dispose();
		}

		protected override unsafe int InitializeShadowVao(uint stride)
		{
			glVertexAttribPointer(0, 3, GL_FLOAT, false, stride, (void*)0);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, stride, (void*)(sizeof(float) * 3));
			glVertexAttribPointer(2, 1, GL_FLOAT, false, stride, (void*)(sizeof(float) * 5));
			glVertexAttribPointer(3, 2, GL_FLOAT, false, stride, (void*)(sizeof(float) * 9));
			glVertexAttribPointer(4, 2, GL_UNSIGNED_SHORT, false, stride, (void*)(sizeof(float) * 11));

			return 5;
		}

		protected override float[] GetData(Mesh mesh)
		{
			var points = mesh.Points;
			var sources = mesh.Sources;
			var normals = mesh.Normals;
			var vertices = mesh.Vertices;
			var boneIndexes = mesh.BoneIndexes;
			var boneWeights = mesh.BoneWeights;

			// The skeletal shader uses ten floats and two shorts, which take up the same combined space as 11 floats.
			var buffer = new float[vertices.Length * 12];

			for (int i = 0; i < vertices.Length; i++)
			{
				var v = vertices[i];
				var p = points[v.x];
				var s = sources[v.y];
				var n = normals[v.w];

				// Positions, normals, and source coordinates are stored in distinct lists, then indexed by vertex. In
				// contrast, bone data is stored directly (i.e. one entry per vertex).
				var w = boneWeights[i];
				var d = boneIndexes[i];
				var start = i * 12;
				var b1 = BitConverter.GetBytes((short)d.x);
				var b2 = BitConverter.GetBytes((short)d.y);

				buffer[start] = p.x;
				buffer[start + 1] = p.y;
				buffer[start + 2] = p.z;
				buffer[start + 3] = s.x;
				buffer[start + 4] = s.y;
				buffer[start + 5] = v.z;
				buffer[start + 6] = n.x;
				buffer[start + 7] = n.y;
				buffer[start + 8] = n.z;
				buffer[start + 9] = w.x;
				buffer[start + 10] = w.y;

				// Both indexes (interpreted as shorts by OpenGL) are combined into the same float.
				buffer[start + 11] = BitConverter.ToSingle(new[] { b1[0], b1[1], b2[0], b2[1] }, 0);
			}

			return buffer;
		}

		public override void Draw(Skeleton item, mat4? vp)
		{
			var bones = item.Bones;
			var bonePositions = bones.Select(b => b.Position.ResultValue).ToArray();
			var boneOrientations = item.Bones.Select(b => b.Orientation.ResultValue.ToVec4()).ToArray();
			var activeShader = vp.HasValue ? shader : shadowShader;

			activeShader.SetUniform("poseOrigin", item.PoseOrigin);
			activeShader.SetUniform("defaultPose", item.DefaultPose);
			activeShader.SetUniform("bonePositions", bonePositions);
			activeShader.SetUniform("boneOrientations", boneOrientations);

			base.Draw(item, vp);
		}
	}
}

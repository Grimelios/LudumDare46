using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Engine.Content;
using Engine.Core;
using Engine.Graphics._3D.Loaders;
using GlmSharp;

namespace Engine.Graphics._3D
{
	public class Mesh
	{
		public delegate bool RaycastFilter(vec3[] triangle, vec3 p, vec3 n, float t);

		private static Texture defaultTexture;

		static Mesh()
		{
			defaultTexture = new Texture(1, 1, new Color(200));
		}

		public static Dictionary<string, Mesh> Load(string filename)
		{
			Debug.Assert(File.Exists(Paths.Meshes + filename), $"Missing mesh '{filename}'.");

			if (filename.EndsWith(".obj"))
			{
				return ObjLoader.Load(filename);
			}

			var mesh = DaeLoader.Load(filename);

			return new Dictionary<string, Mesh>
			{
				{ "", mesh }
			};
		}

		public Mesh(vec3[] points, vec2[] sources, vec3[] normals, ivec4[] vertices, ushort[] indices,
			string[] textures, Material[] materials, ivec2[] boneIndexes = null, vec2[] boneWeights = null)
		{
			var minX = points.Min(p => p.x);
			var maxX = points.Max(p => p.x);
			var minY = points.Min(p => p.y);
			var maxY = points.Max(p => p.y);
			var minZ = points.Min(p => p.z);
			var maxZ = points.Max(p => p.z);
			var width = maxX - minX;
			var height = maxY - minY;
			var depth = maxZ - minZ;

			Points = points;
			Sources = sources;
			Normals = normals;
			Vertices = vertices;
			Indices = indices;
			MaxIndex = indices.Max();

			// TODO: Once meshes are finalized, bounds could be pre-computed.
			Bounds = new vec3(width, height, depth);
			Origin = new vec3(minX, minY, minZ);
			BoneIndexes = boneIndexes;
			BoneWeights = boneWeights;

			Textures = textures == null
				? new [] { defaultTexture }
				: textures.Select(t => ContentCache.GetTexture(t)).ToArray();

			Materials = materials;
		}

		internal MeshHandle Handle { get; set; }

		public vec3[] Points { get; }
		public vec2[] Sources { get; }
		public vec3[] Normals { get; }
		public vec3 Bounds { get; }
		public vec3 Origin { get; }

		// Each vertex is ordered position (X), source (Y), source index (Z), and normal (W).
		public ivec4[] Vertices { get; }

		// These two arrays will be null for non-animated meshes. Bone data can optionally be set programatically
		// (rather than loaded from a file).
		public ivec2[] BoneIndexes { get; set; }
		public vec2[] BoneWeights { get; set; }

		public ushort[] Indices { get; }
		public ushort MaxIndex { get; }

		public Texture[] Textures { get; }
		public Material[] Materials { get; }
	}
}

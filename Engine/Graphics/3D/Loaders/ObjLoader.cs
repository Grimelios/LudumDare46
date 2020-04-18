using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlmSharp;

namespace Engine.Graphics._3D.Loaders
{
	public static class ObjLoader
	{
		public static Dictionary<string, Mesh> Load(string filename)
		{
			var lines = File.ReadAllLines(Paths.Meshes + filename);

			// The default Blender export uses individual objects for each mesh (lines staring with "o"). Object groups
			// start with "g" instead, but are functionally the same (for now, anyway).
			var usesObjectGroups = lines.Any(l => l.StartsWith("g ") || l.StartsWith("o "));

			Dictionary<string, Mesh> results;

			if (usesObjectGroups)
			{
				// TODO: Load textures for meshes with object groups (if needed).
				results = null;// ParseGroupedMeshes(lines);
			}
			else
			{
				// If object groups aren't used, all meshes in the file are combined into a single larger mesh.
				results = new Dictionary<string, Mesh>();
				results.Add("", ParseUngroupedMesh(filename, lines));
			}

			return results;
		}

		/*
		private static Dictionary<string, Mesh> ParseGroupedMeshes(string[] lines)
		{
			var map = new Dictionary<string, Mesh>();
			var index = 0;

			Mesh mesh;

			while ((mesh = ParseMesh(lines, ref index, out var name)) != null)
			{
				map.Add(name, mesh);
			}

			return map;
		}

		private static Mesh ParseMesh(string[] lines, ref int index, out string name)
		{
			// This means that the end of the file was reached.
			if (index >= lines.Length - 1 || lines[index].Length == 0)
			{
				name = null;

				return null;
			}

			string line = lines[index];

			while (!(line.StartsWith("o ") || line.StartsWith("g ")))
			{
				line = lines[++index];
			}

			name = line.Split(' ')[1];

			// Custom object names often end with "_[Shape].XYZ", such as "Box_Cube.001". In those cases, the user-set
			// name is just the first portion. The last underscore index is used in case the object name itself
			// contains underscores.
			var underscoreIndex = name.LastIndexOf("_", StringComparison.CurrentCulture);

			if (underscoreIndex > 0)
			{
				name = name.Substring(0, underscoreIndex);
			}

			var points = new List<vec3>();
			var source = new List<vec2>();
			var normals = new List<vec3>();

			// Parse points.
			do
			{
				line = lines[++index];

				if (line.StartsWith("v "))
				{
					points.Add(ParseVec3(line));
				}
			}
			while (line.StartsWith("v "));

			// Parse textures.
			do
			{
				line = lines[++index];

				if (line.StartsWith("vt"))
				{
					source.Add(ParseVec2(line));
				}
			}
			while (line.StartsWith("vt"));

			// If the exported mesh is untextured, a default grey texture is used.
			if (source.Count == 0)
			{
				source.Add(vec2.Zero);
			}

			// Parse normals.
			do
			{
				line = lines[++index];

				if (line.StartsWith("vn"))
				{
					points.Add(ParseVec3(line));
				}
			}
			while (line.StartsWith("vn"));

			// Advance to face lines.
			//while (!lines[++index].StartsWith("f "))
			{
			}

			var vertices = new List<ivec3>();
			var indices = new List<ushort>();

			while (index < lines.Length - 1 && (line = lines[index]).StartsWith("f "))
			{
				var tokens = line.Split(' ');

				for (int i = 1; i <= 3; i++)
				{
					var subTokens = tokens[i].Split('/');

					// TODO: Support textures (using the second sub-token).
					// .obj files use 1-indexing (rather than 0-indexing).
					var pointIndex = int.Parse(subTokens[0]) - 1;
					var normalIndex = int.Parse(subTokens[2]) - 1;
					var vertex = new ivec3(pointIndex, 0, normalIndex);

					int vIndex;

					if ((vIndex = vertices.IndexOf(vertex)) != -1)
					{
						indices.Add((ushort)vIndex);
					}
					else
					{
						indices.Add((ushort)vertices.Count);
						vertices.Add(vertex);
					}
				}

				index++;
			}

			// When multiple meshes are present in a Blender scene, vertices are based on absolute position. In order
			// to make positioning the mesh easier in-game, points are normalized back to the origin.
			var minX = points.Min(p => p.x);
			var minY = points.Min(p => p.y);
			var minZ = points.Min(p => p.z);

			for (int i = 0; i < points.Count; i++)
			{
				points[i] -= new vec3(minX, minY, minZ);
			}

			// Same applies to indexes.
			var minPointIndex = vertices.Min(v => v.x);
			var minNormalIndex = vertices.Min(v => v.z);

			for (int i = 0; i < vertices.Count; i++)
			{
				var v = vertices[i];
				v.x -= minPointIndex;
				v.z -= minNormalIndex;
				vertices[i] = v;
			}

			// TODO: Load texture (if present in the file).
			return new Mesh(points.ToArray(), source.ToArray(), normals.ToArray(), vertices.ToArray(),
				indices.ToArray(), null);
		}
		*/

		private static Mesh ParseUngroupedMesh(string filename, string[] lines)
		{
			var points = lines.Where(l => l.StartsWith("v ")).Select(ParseVec3);
			var normals = lines.Where(l => l.StartsWith("vn")).Select(ParseVec3);
			var sources = lines.Where(l => l.StartsWith("vt")).Select(ParseVec2).ToList();

			// Source coordinates need to be flipped vertically in order to render properly.
			for (int i = 0; i < sources.Count; i++)
			{
				var s = sources[i];
				s.y = 1 - s.y;
				sources[i] = s;
			}

			bool isUvMapped;
			string[] textures = null;
			Material[] materials = null;
			Dictionary<string, int> materialIndexes = null;

			if (sources.Count > 0)
			{
				ParseMaterials(filename, out textures, out materials, out materialIndexes);
				isUvMapped = true;
			}
			// If no material is specified, a default grey texture is used instead.
			else
			{
				sources.Add(vec2.Zero);
				isUvMapped = false;
			}

			var vertices = new List<ivec4>();
			var indices = new List<ushort>();
			var lineIndex = 0;

			if (isUvMapped)
			{
				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].StartsWith("usemtl"))
					{
						lineIndex = i - 1;

						break;
					}
				}
			}
			else
			{
				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].StartsWith("s "))
					{
						lineIndex = i;

						break;
					}
				}
			}

			// Parse faces (indexes and vertices).
			do
			{
				var name = lines[lineIndex].Split(' ')[1];
				var materialIndex = isUvMapped ? materialIndexes[name] : 0;

				if (isUvMapped)
				{
					lineIndex++;
				}

				do
				{
					var line = lines[++lineIndex];

					// Smoothing lines can be ignored (since smoothing is implied through vertex order, I think).
					if (line.StartsWith("s "))
					{
						continue;
					}

					var tokens = line.Split(' ');

					for (int i = 1; i <= 3; i++)
					{
						var subTokens = tokens[i].Split('/');

						// .obj files use 1-indexing (rather than 0-indexing).
						var pointIndex = int.Parse(subTokens[0]) - 1;
						var sourceIndex = isUvMapped ? int.Parse(subTokens[1]) - 1 : 0;
						var normalIndex = int.Parse(subTokens[2]) - 1;
						var vertex = new ivec4(pointIndex, sourceIndex, materialIndex, normalIndex);

						int index;

						if ((index = vertices.IndexOf(vertex)) != -1)
						{
							indices.Add((ushort)index);
						}
						else
						{
							indices.Add((ushort)vertices.Count);
							vertices.Add(vertex);
						}
					}
				}
				while (lineIndex < lines.Length - 1 && !lines[lineIndex + 1].StartsWith("usemtl"));
			}
			while (lineIndex < lines.Length - 1);

			return new Mesh(points.ToArray(), sources.ToArray(), normals.ToArray(), vertices.ToArray(),
				indices.ToArray(), textures, materials);
		}

		private static vec2 ParseVec2(string line)
		{
			var tokens = line.Split(' ');
			var x = float.Parse(tokens[1]);
			var y = float.Parse(tokens[2]);

			return new vec2(x, y);
		}

		private static vec3 ParseVec3(string line)
		{
			var tokens = line.Split(' ');
			var x = float.Parse(tokens[1]);
			var y = float.Parse(tokens[2]);
			var z = float.Parse(tokens[3]);

			return new vec3(x, y, z);
		}

		private static void ParseMaterials(string meshFilename, out string[] textures, out Material[] materials,
			out Dictionary<string, int> materialIndexes)
		{
			// This assumes that every material file will use the same as the corresponding mesh (with the ".mtl"
			// extension). It also assumes the mesh + material will be in the same directory.
			var lines = File.ReadAllLines(Paths.Meshes + meshFilename.StripExtension() + ".mtl");
			var specularLines = lines.Where(s => s.StartsWith("Ks")).ToArray();
			var textureLines = lines.Where(s => s.StartsWith("map_Kd")).ToArray();
			var nameLines = lines.Where(s => s.StartsWith("newmtl")).ToArray();

			textures = new string[textureLines.Length];
			materials = new Material[textures.Length];
			materialIndexes = new Dictionary<string, int>();

			for (int i = 0; i < textures.Length; i++)
			{
				var name = nameLines[i].Split(' ')[1];

				materialIndexes.Add(name, i);

				var specular = float.Parse(specularLines[i].Split(' ')[1]);
				var tokens = textureLines[i].Split(' ')[1].Split('\\');

				string texture = null;

				// This loop ensures that subfolders (under Textures) are correctly parsed.
				for (int j = 0; j < tokens.Length; j++)
				{
					if (tokens[j] == "Textures")
					{
						var path = new List<string>();

						for (int k = j + 1; k < tokens.Length; k++)
						{
							path.Add(tokens[k]);
						}

						texture = string.Join("/", path);

						break;
					}
				}

				textures[i] = texture;
				materials[i] = new Material(specular);
			}
		}
	}
}

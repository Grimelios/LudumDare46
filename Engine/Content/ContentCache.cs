using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Graphics;
using Engine.Graphics._2D;
using Engine.Graphics._3D;
using Engine.UI;

namespace Engine.Content
{
	public static class ContentCache
	{
		private static Dictionary<string, SpriteFont> fonts;
		private static Dictionary<string, Mesh> meshes;
		private static Dictionary<string, Texture> textures;
		private static Dictionary<string, Atlas> atlases;

		static ContentCache()
		{
			fonts = new Dictionary<string, SpriteFont>();
			meshes = new Dictionary<string, Mesh>();
			textures = new Dictionary<string, Texture>();
			atlases = new Dictionary<string, Atlas>();
		}

		public static Mesh GetMesh(string filename, string meshName = null, bool shouldCache = true)
		{
			Debug.Assert(!string.IsNullOrEmpty(filename), "Mesh filename can't be null or empty.");
			Debug.Assert(meshName != "", "Mesh name can't be empty (it can be null).");

			var pipeIndex = filename.IndexOf('|');

			if (pipeIndex >= 0)
			{
				Debug.Assert(meshName == null, "Duplicate mesh names specified (use either the meshName parameter " +
					"or a piped filename, but not both).");

				var tokens = filename.Split('|');

				Debug.Assert(tokens.Length == 2, $"Invalid named mesh format '{filename}' (to specify a named mesh, " +
					"use filename|mesh).");

				filename = tokens[0] + ".obj";
				meshName = tokens[1].StripExtension();

				Debug.Assert(filename.Length > 4, $"After splitting a named mesh, filename was empty ('{filename}').");
				Debug.Assert(meshName.Length > 0, $"After splitting a named mesh, mesh name was empty ('{meshName}').");
			}

			var key = filename + (meshName != null ? $"|{meshName}" : "");

			if (!meshes.TryGetValue(key, out var mesh))
			{
				var results = Mesh.Load(filename);

				if (meshName != null)
				{
					if (!results.ContainsKey(meshName))
					{
						Debug.Fail($"Mesh '{meshName}' does not exist in file '{filename}'.");
					}

					mesh = results[meshName];
				}
				else if (results.Any(e => e.Key.Length > 0))
				{
					Debug.Fail($"File '{filename}' uses object groups. Must select a named mesh.");
				}
				else
				{
					// If a mesh name isn't given, the full .obj file is parsed into a single mesh.
					mesh = results.First().Value;
				}

				if (shouldCache)
				{
					foreach (var pair in results)
					{
						// A key of an empty string means that the .obj file wasn't exported using object groups.
						key = filename + (pair.Key.Length > 0 ? $"|{pair.Key}" : "");
						meshes.Add(key, pair.Value);
					}
				}
			}
			
			return mesh;
		}

		public static void RemoveMesh(Mesh mesh)
		{
			meshes.Remove(meshes.First(pair => pair.Value == mesh).Key);
		}

		public static SpriteFont GetFont(string name, bool shouldCache = true)
		{
			Debug.Assert(!string.IsNullOrEmpty(name), "Font name can't be null or empty.");

			if (!fonts.TryGetValue(name, out SpriteFont font))
			{
				font = SpriteFont.Load(name, shouldCache);

				if (shouldCache)
				{
					fonts.Add(name, font);
				}
			}

			return font;
		}

		public static void RemoveFont(SpriteFont font)
		{
			fonts.Remove(fonts.First(pair => pair.Value == font).Key);
		}

		public static Texture GetTexture(string filename, bool shouldStoreData = false, bool shouldCache = true,
			TextureWraps wrap = TextureWraps.Repeat, TextureFilters filter = TextureFilters.Nearest,
			string folder = "Textures/")
		{
			Debug.Assert(!string.IsNullOrEmpty(filename), "Texture filename can't be null or empty.");
			Debug.Assert(!string.IsNullOrEmpty(folder), "Texture folder can't be null or empty.");

			if (!textures.TryGetValue(filename, out var texture))
			{
				texture = Texture.Load(filename, folder, shouldStoreData, wrap, filter);

				if (shouldCache)
				{
					textures.Add(filename, texture);
				}
			}

			return texture;
		}

		public static void RemoveTexture(Texture texture)
		{
			textures.Remove(textures.First(pair => pair.Value == texture).Key);
		}

		public static Atlas GetAtlas(string filename, bool shouldCache = true)
		{
			Debug.Assert(!string.IsNullOrEmpty(filename), "Atlas filename can't be null or empty.");

			if (!atlases.TryGetValue(filename, out var atlas))
			{
				atlas = new Atlas(filename);

				if (shouldCache)
				{
					atlases.Add(filename, atlas);
				}
			}

			return atlas;
		}

		public static void RemoveAtlas(Atlas atlas)
		{
			atlases.Remove(atlases.First(pair => pair.Value == atlas).Key);
		}
	}
}

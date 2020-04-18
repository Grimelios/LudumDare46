using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Interfaces._3D;
using Engine.Lighting;
using Engine.Shaders;
using Engine.View._3D;
using GlmSharp;
using static Engine.Graphics.GL;

namespace Engine.Graphics._3D.Rendering
{
	public abstract class AbstractRenderer3D<K, V> : IDisposable where V : IRenderable3D
	{
		private uint shadowVao;

		private int nextIndex;

		// A separate list is needed to access keys by index.
		private List<K> keys;
		private Dictionary<K, List<V>> map;

		protected uint bufferId;
		protected uint indexId;

		protected Shader shader;

		protected AbstractRenderer3D(Camera3D camera, GlobalLight3D light)
		{
			Debug.Assert(camera != null, "3D renderers must be created with a non-null camera.");
			Debug.Assert(light != null, "3D renderers must be created with a non-null global light class.");

			Camera = camera;
			Light = light;
			keys = new List<K>();
			map = new Dictionary<K, List<V>>();
		}

		protected Camera3D Camera { get; }
		protected GlobalLight3D Light { get; }

		// Skeletons use a custom shadow shadow (but it's simpler and more future-proof to put the property here).
		public virtual Shader ShadowShader => null;

		// This is used for debug purposes.
		public int Size => map.Sum(pair => pair.Value.Count);

		protected unsafe void Bind(uint bufferId, uint indexId)
		{
			this.bufferId = bufferId;
			this.indexId = indexId;

			shader.Initialize();
			shader.Bind(bufferId, indexId);
			shader.Use();

			if (shader.HasUniform("textureSamplers"))
			{
				shader.SetUniform("textureSamplers", Enumerable.Range(0, Constants.TextureLimit).ToArray());
			}

			shader.SetUniform("shadowSampler", Constants.TextureLimit);

			var stride = shader.Stride;

			fixed (uint* address = &shadowVao)
			{
				glGenVertexArrays(1, address);
			}

			glBindVertexArray(shadowVao);

			// Using this function allows the skeletal renderer to set up its custom shadow shader properly.
			var attributeCount = InitializeShadowVao(stride);

			for (int i = 0; i < attributeCount; i++)
			{
				glEnableVertexAttribArray((uint)i);
			}
		}

		protected virtual unsafe int InitializeShadowVao(uint stride)
		{
			glVertexAttribPointer(0, 3, GL_FLOAT, false, stride, (void*)0);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, stride, (void*)(sizeof(float) * 3));

			// This feels a bit hacky, but it should work.
			if (!(this is SpriteBatch3D))
			{
				glVertexAttribPointer(2, 1, GL_FLOAT, false, stride, (void*)(sizeof(float) * 5));

				return 3;
			}

			return 2;
		}

		public virtual void Dispose()
		{
			// Renderers don't own their associated buffers, so they don't need to be deleted here.
			shader.Dispose();
		}

		public abstract void Add(V item);
		public abstract void Remove(V item);

		protected void Add(K key, V item)
		{
			Debug.Assert(item != null, "Can't add a null renderable item.");

			if (!map.TryGetValue(key, out var list))
			{
				list = new List<V>();
				map.Add(key, list);
				keys.Add(key);
			}

			list.Add(item);
		}

		protected void Remove(K key, V item)
		{
			map[key].Remove(item);
		}

		public List<V> RetrieveNext()
		{
			if (nextIndex < keys.Count)
			{
				var key = keys[nextIndex++];

				// This call allows binding any relevant open GL state before drawing begins.
				Apply(key);

				return map[key];
			}

			// This resets the renderer for the next phase.
			nextIndex = 0;

			return null;
		}

		protected abstract void Apply(K key);

		public virtual void PrepareShadow()
		{
			glBindVertexArray(shadowVao);
			glBindBuffer(GL_ARRAY_BUFFER, bufferId);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexId);
		}

		public virtual void Prepare()
		{
			shader.Apply();
			shader.SetUniform("lightDirection", Light.Direction);
			shader.SetUniform("lightColor", Light.Color.ResultValue.ToVec3());
			shader.SetUniform("ambientIntensity", Light.AmbientIntensity);
		}

		protected void PrepareShader(V item, mat4 vp)
		{
			var world = item.WorldMatrix;
			var orientation = item.Orientation.ResultValue;

			if (shader.HasUniform("mvp"))
			{
				shader.SetUniform("mvp", vp * world);
			}
			else
			{
				shader.SetUniform("worldMatrix", world);
				shader.SetUniform("vpMatrix", vp);
			}

			shader.SetUniform("orientation", orientation.ToMat4);
			shader.SetUniform("lightBiasMatrix", Light.BiasMatrix * world);
		}

		public abstract void Draw(V item, mat4? vp);
	}
}

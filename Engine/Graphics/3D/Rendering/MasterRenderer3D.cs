using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Animation;
using Engine.Core;
using Engine.Core._3D;
using Engine.Interfaces._3D;
using Engine.Lighting;
using Engine.Shaders;
using Engine.View._3D;
using GlmSharp;

namespace Engine.Graphics._3D.Rendering
{
	public class MasterRenderer3D : IRenderTargetUser3D
	{
		private Camera3D camera;
		private Shader shadowMapShader;
		private RenderTarget shadowMapTarget;
		private ModelRenderer modelRenderer;
		private SpriteBatch3D spriteBatch3D;
		private SkeletonRenderer skeletonRenderer;

		public MasterRenderer3D(Camera3D camera)
		{
			this.camera = camera;

			// Note that shader attributes aren't set here since custom VAOs are created per renderer.
			shadowMapShader = new Shader();
			shadowMapShader.Attach(ShaderTypes.Vertex, "ShadowMap.vert");
			shadowMapShader.Attach(ShaderTypes.Fragment, "ShadowMap.frag");
			shadowMapShader.Initialize();
			shadowMapShader.Use();
			shadowMapShader.SetUniform("images", Enumerable.Repeat(0, Constants.TextureLimit).ToArray());

			Light = new GlobalLight3D();

			shadowMapTarget = new RenderTarget(2048, 2048, RenderTargetFlags.Depth);
			modelRenderer = new ModelRenderer(camera, Light);
			spriteBatch3D = new SpriteBatch3D(camera, Light);
			skeletonRenderer = new SkeletonRenderer(camera, Light, 100000, 10000);

			IsDrawEnabled = true;
		}

		// These are used for debug purposes.
		internal ModelRenderer Models => modelRenderer;
		internal SpriteBatch3D Sprites => spriteBatch3D;
		internal SkeletonRenderer Skeletons => skeletonRenderer;

		public Camera3D Camera
		{
			set
			{
				Debug.Assert(value != null, "Can't nullify the camera on the master 3D renderer.");

				camera = value;
			}
		}

		public GlobalLight3D Light { get; }

		// Exposing this publicly is useful for shadow map visualization.
		public RenderTarget ShadowTarget => shadowMapTarget;

		public bool IsDrawEnabled { get; set; }

		public void Dispose()
		{
			shadowMapShader.Dispose();
			shadowMapTarget.Dispose();
			spriteBatch3D.Dispose();
			modelRenderer.Dispose();
			skeletonRenderer.Dispose();
		}

		public void Add(Model model)
		{
			modelRenderer.Add(model);
		}

		public void Add(Sprite3D sprite)
		{
			spriteBatch3D.Add(sprite);
		}

		public void Add(Skeleton skeleton)
		{
			skeletonRenderer.Add(skeleton);
		}

		public void Remove(Model model)
		{
			modelRenderer.Remove(model);
		}

		public void Remove(Sprite3D sprite)
		{
			spriteBatch3D.Remove(sprite);
		}

		public void Remove(Skeleton skeleton)
		{
			skeletonRenderer.Remove(skeleton);
		}

		public void DrawTargets(float t)
		{
			if (!IsDrawEnabled)
			{
				return;
			}

			Light.Recompute(t);
			Light.RecomputeMatrices(camera);

			shadowMapTarget.Apply();

			DrawShadow(modelRenderer, t);
			DrawShadow(spriteBatch3D, t);
			DrawShadow(skeletonRenderer, t);
		}

		private void DrawShadow<K, V>(AbstractRenderer3D<K, V> renderer, float t) where V : IRenderable3D
		{
			// Skeletons use a custom shadow shader (since skeletal vertices are transformed differently).
			var shadowShader = renderer.ShadowShader ?? shadowMapShader;

			renderer.PrepareShadow();

			List<V> items;

			while ((items = renderer.RetrieveNext()) != null)
			{
				shadowShader.Use();

				foreach (V item in items)
				{
					if (!item.IsDrawEnabled)
					{
						continue;
					}

					// Even if the object doesn't cast a shadow, its world matrix is recomputed here for use during
					// normal rendering.
					item.Recompute(t);

					if (!item.IsShadowCaster)
					{
						continue;
					}

					shadowShader.SetUniform("lightMatrix", Light.Matrix * item.WorldMatrix);
					renderer.Draw(item, null);
				}
			}
		}

		public void Draw()
		{
			if (!IsDrawEnabled)
			{
				return;
			}

			shadowMapTarget.Bind(Constants.TextureLimit);

			Draw(modelRenderer);
			Draw(spriteBatch3D);
			Draw(skeletonRenderer);
		}

		private void Draw<K, V>(AbstractRenderer3D<K, V> renderer) where V : IRenderable3D
		{
			renderer.Prepare();

			List<V> items;

			while ((items = renderer.RetrieveNext()) != null)
			{
				foreach (V item in items)
				{
					renderer.Draw(item, camera.ViewProjection);
				}
			}
		}
	}
}

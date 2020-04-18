using System;
using System.Collections.Generic;
using Engine.Components;
using Engine.Core._2D;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Utility;
using GlmSharp;

namespace LD46.Entities.Core
{
	public abstract class SimpleEntity2D : IPositionContainer2D, IRotationContainer, IDynamic, IDisposable
	{
		private vec2 position;

		private float rotation;

		private List<(Component2D Component, vec2 Position, float Rotation)> attachments;
		private ComponentCollection components;

		protected SimpleEntity2D()
		{
			attachments = new List<(Component2D Component, vec2 Position, float Rotation)>();
			IsUpdateEnabled = true;
			IsDrawEnabled = true;
		}

		public ComponentCollection Components => components ??= new ComponentCollection();

		protected T Attach<T>(T component, vec2? p = null, float? r = null) where T : Component2D
		{
			var pPrime = p ?? vec2.Zero;
			var rPrime = r ?? 0;

			attachments.Add((component, pPrime, rPrime));

			var sum = rotation + rPrime;

			component.Position.SetValue(position + Utilities.Rotate(pPrime, sum), false);
			component.Rotation.SetValue(sum, false);

			return component;
		}

		public vec2 Position => position;

		public float Rotation => rotation;

		public bool IsUpdateEnabled { get; set; }
		public bool IsDrawEnabled { get; set; }

		public SimpleScene2D Scene { get; set; }

		public void SetPosition(vec2 position, bool shouldInterpolate)
		{
			this.position = position;

			attachments.ForEach(a =>
			{
				var (component, p, r) = a;

				component.Position.SetValue(position + Utilities.Rotate(p, rotation + r), shouldInterpolate);
			});
		}

		public void SetRotation(float rotation, bool shouldInterpolate)
		{
			this.rotation = rotation;

			attachments.ForEach(a =>
			{
				var (component, p, r) = a;
				var rPrime = rotation + r;

				component.Position.SetValue(position + Utilities.Rotate(p, rPrime), shouldInterpolate);
				component.Rotation.SetValue(rPrime, shouldInterpolate);
			});
		}

		public void SetTransform(vec2 position, float rotation, bool shouldInterpolate)
		{
			this.position = position;
			this.rotation = rotation;

			attachments.ForEach(a =>
			{
				var (component, p, r) = a;
				var rPrime = rotation + r;

				component.Position.SetValue(position + Utilities.Rotate(p, rPrime), shouldInterpolate);
				component.Rotation.SetValue(rPrime, shouldInterpolate);
			});
		}

		public void Dispose()
		{
			attachments.ForEach(a => a.Component.Dispose());
		}

		public virtual void Update()
		{
			components?.Update();
		}

		public virtual void Draw(SpriteBatch sb, float t)
		{
			attachments.ForEach(a =>
			{
				var component = a.Component;

				if (component.IsDrawEnabled)
				{
					component.Draw(sb, t);
				}
			});
		}
	}
}

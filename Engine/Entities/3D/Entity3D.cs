using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Content;
using Engine.Core._2D;
using Engine.Core._3D;
using Engine.Graphics;
using Engine.Graphics._3D;
using Engine.Interfaces._3D;
using Engine.Physics._3D;
using Engine.Sensors;
using Engine.Sensors._3D;
using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Entities._3D
{
	public abstract class Entity3D<TGroup> : Entity<Entity3D<TGroup>, EntityHandle3D<TGroup>, TGroup, Scene3D<TGroup>,
		World3D, Sensor3D, Shape3D, ShapeTypes3D, Space3D, vec3>, ITransformContainer3D where TGroup : struct
	{
		private List<(EntityAttachmentTypes3D Type, ITransformable3D Target, vec3 Position,
			quat Orientation)> attachments;
		private List<(EntityAttachmentTypes3D Type, IRenderTransformable3D Target, vec3 Position,
			quat Orientation)> renderAttachments;

		private quat orientation;

		protected Entity3D(TGroup group) : base(group)
		{
			orientation = quat.Identity;
			attachments = new List<(EntityAttachmentTypes3D Type, ITransformable3D Target, vec3 Position,
				quat Orientation)>();
			renderAttachments = new List<(EntityAttachmentTypes3D Type, IRenderTransformable3D Target, vec3 Position,
				quat Orientation)>();
		}

		public quat Orientation => orientation;

		public override void SetPosition(vec3 position, bool shouldInterpolate)
		{
			base.SetPosition(position, shouldInterpolate);

			attachments.ForEach(a => a.Target.Position = position + orientation * a.Position);
			renderAttachments.ForEach(a => a.Target.Position.SetValue(position + orientation * a.Position,
				shouldInterpolate));
		}

		public void SetOrientation(quat orientation, bool shouldInterpolate)
		{
			this.orientation = orientation;

			flags |= EntityFlags.IsOrientationSet;

			attachments.ForEach(a =>
			{
				var (_, target, p, q) = a;
				target.Position = position + orientation * p;
				target.Orientation = orientation * q;
			});

			renderAttachments.ForEach(a =>
			{
				var (_, target, p, q) = a;
				target.Position.SetValue(position + orientation * p, shouldInterpolate);
				target.Orientation.SetValue(orientation * q, shouldInterpolate);
			});
		}

		public void SetTransform(vec3 position, quat orientation, bool shouldInterpolate)
		{
			this.position = position;
			this.orientation = orientation;

			flags |= EntityFlags.IsPositionSet | EntityFlags.IsOrientationSet;

			attachments.ForEach(a =>
			{
				var (_, target, p, q) = a;
				target.Position = position + orientation * p;
				target.Orientation = orientation * q;
			});

			renderAttachments.ForEach(a =>
			{
				var (_, target, p, q) = a;
				target.Position.SetValue(position + orientation * p, shouldInterpolate);
				target.Orientation.SetValue(orientation * q, shouldInterpolate);
			});
		}

		// Note that for this callback, the point is the position on the triangle, not the entity.
		public virtual bool OnContact(vec3 p, vec3 n, vec3[] triangle)
		{
			return true;
		}

		private void Attach(EntityAttachmentTypes3D type, ITransformable3D target, vec3? p, quat? q)
		{
			var pPrime = p ?? vec3.Zero;
			var qPrime = q ?? quat.Identity;

			attachments.Add((type, target, pPrime, qPrime));

			target.Position = position + orientation * pPrime;
			target.Orientation = orientation * qPrime;
		}

		private void Attach(EntityAttachmentTypes3D type, IRenderTransformable3D target, vec3? p, quat? q)
		{
			var pPrime = p ?? vec3.Zero;
			var qPrime = q ?? quat.Identity;

			renderAttachments.Add((type, target, pPrime, qPrime));

			target.Position.SetValue(position + orientation * pPrime, false);
			target.Orientation.SetValue(orientation * qPrime, false);
				
			((IRenderable3D)target).IsDrawEnabled = IsDrawEnabled;
		}

		private void ComputeFan(int count, vec3 p, quat? q, vec3 axis, out vec3[] positions, out quat[] orientations)
		{
			Debug.Assert(count > 0, "Fan count must be positive.");

			// Passing an orientation allows each fanned object to be rotated. It does NOT affect computed positions
			// around the axis.
			var increment = Constants.TwoPi / count;
			var qPrime = q ?? quat.Identity;

			positions = new vec3[count];
			orientations = new quat[count];

			for (int i = 0; i < count; i++)
			{
				var qItem = quat.FromAxisAngle(increment * i, axis);
				orientations[i] = qItem * qPrime * orientation;
				positions[i] = position + qItem * p;
			}
		}

		protected Sprite3D CreateSprite(Scene3D<TGroup> scene, string texture,
			Alignments alignment = Alignments.Center, bool shouldAttach = true, vec3? p = null, quat? q = null)
		{
			return CreateSprite(scene, ContentCache.GetTexture(texture), new SourceRect(), alignment, shouldAttach, p,
				q);
		}

		protected Sprite3D CreateSprite(Scene3D<TGroup> scene, string texture, SourceRect sourceRect,
			Alignments alignment = Alignments.Center, bool shouldAttach = true, vec3? p = null, quat? q = null)
		{
			return CreateSprite(scene, ContentCache.GetTexture(texture), sourceRect, alignment, shouldAttach, p, q);
		}

		protected Sprite3D CreateSprite(Scene3D<TGroup> scene, QuadSource source, ivec2 origin,
			bool shouldAttach = true, vec3? p = null, quat? q = null)
		{
			return AddSprite(scene, new Sprite3D(source, new SourceRect(), origin), shouldAttach, p, q);
		}

		protected Sprite3D CreateSprite(Scene3D<TGroup> scene, QuadSource source, SourceRect sourceRect, ivec2 origin,
			bool shouldAttach = true, vec3? p = null, quat? q = null)
		{
			return AddSprite(scene, new Sprite3D(source, sourceRect, origin), shouldAttach, p, q);
		}

		protected Sprite3D CreateSprite(Scene3D<TGroup> scene, QuadSource source,
			Alignments alignment = Alignments.Center, bool shouldAttach = true, vec3? p = null, quat? q = null)
		{
			return AddSprite(scene, new Sprite3D(source, new SourceRect(), alignment), shouldAttach, p, q);
		}

		protected Sprite3D CreateSprite(Scene3D<TGroup> scene, QuadSource source, SourceRect sourceRect,
			Alignments alignment = Alignments.Center, bool shouldAttach = true, vec3? p = null, quat? q = null)
		{
			return AddSprite(scene, new Sprite3D(source, sourceRect, alignment), shouldAttach, p, q);
		}

		private Sprite3D AddSprite(Scene3D<TGroup> scene, Sprite3D sprite, bool shouldAttach, vec3? p = null,
			quat? q = null)
		{
			scene.Renderer.Add(sprite);

			if (shouldAttach)
			{
				Attach(EntityAttachmentTypes3D.Sprite, sprite, p, q);
			}

			return sprite;
		}

		protected Model CreateModel(Scene3D<TGroup> scene, string filename, bool shouldAttach = true, vec3? p = null,
			quat? q = null)
		{
			return CreateModel(scene, ContentCache.GetMesh(filename), shouldAttach, p, q);
		}

		protected Model CreateModel(Scene3D<TGroup> scene, string filename, out vec3 bounds)
		{
			var model = CreateModel(scene, ContentCache.GetMesh(filename));
			bounds = model.Mesh.Bounds;

			return model;
		}

		protected Model CreateModel(Scene3D<TGroup> scene, Mesh mesh, bool shouldAttach = true, vec3? p = null,
			quat? q = null)
		{
			Debug.Assert(mesh != null, "Can't create a model with a null mesh.");

			var model = new Model(mesh);
			scene.Renderer.Add(model);

			if (shouldAttach)
			{
				Attach(EntityAttachmentTypes3D.Model, model, p, q);
			}

			return model;
		}

		protected Model[] FanModels(Scene3D<TGroup> scene, string filename, int count, vec3 p, vec3 axis,
			quat? q = null)
		{
			return FanModels(scene, ContentCache.GetMesh(filename), count, p, axis, q);
		}

		protected Model[] FanModels(Scene3D<TGroup> scene, Mesh mesh, int count, vec3 axis, vec3 p, quat? q = null)
		{
			ComputeFan(count, p, q, axis, out var positions, out var orientations);

			var models = new Model[count];

			for (int i = 0; i < count; i++)
			{
				models[i] = CreateModel(scene, mesh, true, positions[i], orientations[i]);
			}

			return models;
		}

		protected Sensor3D CreateSensor(Scene3D<TGroup> scene, Shape3D shape = null, uint group = 0, uint affects = 0,
			SensorTypes type = SensorTypes.Entity, bool isEnabled = true, bool shouldAttach = true, vec3? p = null,
			quat? q = null)
		{
			var sensor = new Sensor3D(type, this, shape, group, affects);
			sensor.IsEnabled = isEnabled;
			scene.Space.Add(sensor);

			if (shouldAttach)
			{
				Attach(EntityAttachmentTypes3D.Sensor, sensor, p, q);
			}

			return sensor;
		}

		protected Sensor3D[] FanSensors(Scene3D<TGroup> scene, Func<Shape3D> shapeFunction, int count, vec3 axis,
			vec3 p, uint group = 0, uint affects = 0, SensorTypes type = SensorTypes.Entity, quat? q = null)
		{
			ComputeFan(count, p, q, axis, out var positions, out var orientations);

			var sensors = new Sensor3D[count];

			for (int i = 0; i < count; i++)
			{
				sensors[i] = CreateSensor(scene, shapeFunction.Invoke(), group, affects, type, true, true,
					positions[i], orientations[i]);
			}

			return sensors;
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (var (type, target, _, _) in attachments)
			{
				switch (type)
				{
					case EntityAttachmentTypes3D.Sensor:
						Scene.Space.Remove((Sensor3D)target);

						break;
				}
			}

			foreach (var (type, target, _, _) in renderAttachments)
			{
				switch (type)
				{
					case EntityAttachmentTypes3D.Model:
						Scene.Renderer.Remove((Model)target);

						break;
				}
			}
		}
	}
}

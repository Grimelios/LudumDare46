using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine.Interfaces;
using Engine.Sensors;
using Engine.Shapes;
using Engine.UI;

namespace Engine.Entities
{
	public abstract class Scene<TEntity, THandle, TGroup, TScene, TWorld, TSensor, TShape, TShapeType, TSpace, TPosition> :
		IDynamic
		where TEntity : Entity<TEntity, THandle, TGroup, TScene, TWorld, TSensor, TShape, TShapeType, TSpace, TPosition>
		where THandle : EntityHandle<TEntity, THandle, TGroup, TScene, TWorld, TSensor, TShape, TShapeType, TSpace, TPosition>, new()
		where TGroup : struct
		where TScene : Scene<TEntity, THandle, TGroup, TScene, TWorld, TSensor, TShape, TShapeType, TSpace, TPosition>
		where TWorld : class
		where TSensor : Sensor<TSensor, TShape, TShapeType, TSpace, TPosition>
		where TShape : Shape<TShape, TShapeType, TPosition>
		where TShapeType : struct
		where TSpace : Space<TSensor, TShape, TShapeType, TSpace, TPosition>
		where TPosition : struct
	{
		private Dictionary<TGroup, List<TEntity>> entities;
		private List<TEntity> addList;
		private List<TEntity> removeList;

		private bool isUpdateActive;

		protected Scene(Canvas canvas, TWorld world, TSpace space)
		{
			// Both world and space are allowed to be null (although they usually won't be). For example, a scene from
			// a point-and-click adventure game might use sensors, but not physics.
			Debug.Assert(canvas != null, "Scenes must be created with a non-null canvas.");

			Canvas = canvas;
			World = world;
			Space = space;
			entities = new Dictionary<TGroup, List<TEntity>>();
			addList = new List<TEntity>();
			removeList = new List<TEntity>();
		}

		public Canvas Canvas { get; }

		public TWorld World { get; }
		public TSpace Space { get; }

		public T Add<T>(T entity) where T : TEntity
		{
			Debug.Assert(entity != null, "Can't add a null entity.");

			if (isUpdateActive)
			{
				addList.Add(entity);
			}
			else
			{
				AddInternal(entity);
			}

			entity.flags |= EntityFlags.IsAdded;
			entity.flags &= ~EntityFlags.IsRemoved;

			return entity;
		}

		private void AddInternal(TEntity entity)
		{
			var group = entity.Group;

			if (!entities.TryGetValue(group, out var list))
			{
				list = new List<TEntity>();
				entities.Add(group, list);
			}
			else
			{
				Debug.Assert(!list.Contains(entity), "Entity was already added to the scene.");
			}

			list.Add(entity);

			if (!entity.IsInitialized)
			{
				entity.Initialize((TScene)this, null);
			}
		}

		public void Remove(TEntity entity)
		{
			Debug.Assert(entity != null, "Can't remove a null entity.");

			if (isUpdateActive)
			{
				removeList.Add(entity);
			}
			else
			{
				RemoveInternal(entity);
			}
		}

		private void RemoveInternal(TEntity entity)
		{
			var group = entity.Group;
			var flags = entity.flags;

			Debug.Assert((flags & EntityFlags.IsRemoved) == 0, "Entity was already removed.");
			Debug.Assert((flags & EntityFlags.IsAdded) > 0, "Entity was never added to the scene.");

			entity.flags &= ~EntityFlags.IsAdded;
			entity.flags |= EntityFlags.IsRemoved;
			entity.Dispose();

			entities[group].Remove(entity);
		}

		public List<TEntity> GetEntities(TGroup group)
		{
			if (!entities.TryGetValue(group, out var list))
			{
				list = new List<TEntity>();
				entities.Add(group, list);
			}

			return list;
		}

		public List<T> GetEntities<T>(TGroup group) where T : TEntity
		{
			if (!entities.TryGetValue(group, out var list))
			{
				list = new List<TEntity>();
				entities.Add(group, list);
			}

			return list.Where(e => e is T).Cast<T>().ToList();
		}

		public void Update()
		{
			isUpdateActive = true;

			foreach (var list in entities.Values)
			{
				list.ForEach(e =>
				{
					if (e.IsUpdateEnabled)
					{
						e.Update();
					}
				});
			}

			isUpdateActive = false;

			removeList.ForEach(RemoveInternal);
			removeList.Clear();

			addList.ForEach(AddInternal);
			addList.Clear();
		}
	}
}

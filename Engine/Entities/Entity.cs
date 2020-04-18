using System.Collections.Generic;
using System.Diagnostics;
using Engine.Components;
using Engine.Input;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Props;
using Engine.Sensors;
using Engine.Shapes;
using Newtonsoft.Json.Linq;

namespace Engine.Entities
{
	public abstract class Entity<TEntity, THandle, TGroup, TScene, TWorld, TSensor, TShape, TShapeType, TSpace, TPosition> :
		IDynamic, IReloadable
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
		static Entity()
		{
			var type = typeof(TGroup);

			Debug.Assert(type.IsEnum, $"Invalid entity group enumeration {type.FullName}.");
		}

		private ComponentCollection components;
		private List<THandle> handles;

		internal EntityFlags flags;

		protected TPosition position;

		protected Entity(TGroup group)
		{
			Group = group;
			flags = EntityFlags.None;
			IsUpdateEnabled = true;
			IsDrawEnabled = true;
		}

		// ID isn't set on all entities (only those meant to be retrieved via handles).
		internal int Id { get; private set; }

		internal bool IsInitialized => (flags & EntityFlags.IsInitialized) > 0;

		protected TScene Scene { get; private set; }
		protected ComponentCollection Components => components ??= new ComponentCollection();

		public TGroup Group { get; }
		public TPosition Position => position;

		public bool IsUpdateEnabled
		{
			get => (flags & EntityFlags.IsUpdateEnabled) > 0;
			set
			{
				if (value)
				{
					flags |= EntityFlags.IsUpdateEnabled;
				}
				else
				{
					flags &= ~EntityFlags.IsUpdateEnabled;
				}
			}
		}

		public bool IsDrawEnabled
		{
			get => (flags & EntityFlags.IsDrawEnabled) > 0;
			set
			{
				if (value)
				{
					flags |= EntityFlags.IsDrawEnabled;
				}
				else
				{
					flags &= ~EntityFlags.IsDrawEnabled;
				}
			}
		}

		public virtual void Initialize(TScene scene, JToken data)
		{
			Debug.Assert((flags & EntityFlags.IsInitialized) == 0, "Can't initialize an entity more than once.");

			Scene = scene;
			flags |= EntityFlags.IsInitialized;

			if (data == null)
			{
				return;
			}

			// Parse ID.
			if (data.TryParse("Id", out int id))
			{
				Id = id;
			}

			// Parse handles.
			var hToken = data["Handle"];
			var hListToken = data["Handles"];

			Debug.Assert(hToken == null || hListToken == null, "Duplicate entity handle blocks. Use either Handle " +
				"(for singular handles) or Handles (for multiple handles).");

			if (hToken != null || hListToken != null)
			{
				handles = new List<THandle>();

				if (hToken != null)
				{
					var handle = new THandle();
					handle.Load(hToken);
					handles.Add(handle);
				}
				else
				{
					foreach (var token in hListToken.Children())
					{
						var handle = new THandle();
						handle.Load(token);
						handles.Add(handle);
					}
				}
			}
			else
			{
				// Having no handles still counts as being resolved.
				flags |= EntityFlags.HandlesResolved;
			}
		}

		public bool Reload(PropertyAccessor accessor, out string message)
		{
			message = null;

			return true;
		}

		public void ResolveHandles(TScene scene)
		{
			Debug.Assert((flags & EntityFlags.HandlesResolved) == 0, "Entity handles have already been resolved.");

			if (handles != null && handles.Count > 0)
			{
				ResolveHandles(scene, handles);
			}

			handles = null;
			flags |= EntityFlags.HandlesResolved;
		}

		protected virtual void ResolveHandles(TScene scene, List<THandle> handles)
		{
		}

		public virtual void SetPosition(TPosition position, bool shouldInterpolate)
		{
			this.position = position;

			flags |= EntityFlags.IsPositionSet;
		}

		public virtual void Dispose()
		{
			Debug.Assert((flags & EntityFlags.IsAdded) == 0, "Since this entity was added to the scene, it should " +
				"be disposed by calling Scene.Remove (rather than by calling Dispose directly).");
			Debug.Assert((flags & EntityFlags.IsDisposed) == 0, "Entity was already disposed.");

			if (this is IControllable target)
			{
				InputProcessor.Remove(target);
			}

			if (this is IReceiver receiver)
			{
				MessageSystem.Unsubscribe(receiver);
			}
		}

		public virtual void Update()
		{
			components?.Update();
		}
	}
}

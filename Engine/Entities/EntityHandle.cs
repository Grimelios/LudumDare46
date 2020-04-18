using System.Diagnostics;
using System.Linq;
using Engine.Sensors;
using Engine.Shapes;
using Newtonsoft.Json.Linq;

namespace Engine.Entities
{
	public abstract class EntityHandle<TEntity, THandle, TGroup, TScene, TWorld, TSensor, TShape, TShapeType, TSpace,
		TPosition>
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
		private TGroup group;

		private int id;
		private int usage;

		// This used to be a constructor, but was changed to a regular function due to generics in the base Entity
		// class (you can't construct a generic instance using constructor arguments).
		internal void Load(JToken token)
		{
			group = token["Group"].Value<TGroup>();
			id = token["Id"].Value<int>();

			Debug.Assert(id >= 0, "Entity handle ID can't be negative.");

			// Usage is optional and allows entities to distinguish among multiple handles within the same group.
			if (token.TryParse("Usage", out usage))
			{
				Debug.Assert(usage >= 0, "Entity handle usage can't be negative.");
			}
			else
			{
				usage = -1;
			}
		}

		public int Usage => usage;

		public T Resolve<T>(TScene scene) where T : TEntity
		{
			return (T)scene.GetEntities(group).First(e => e.Id == id);
		}
	}
}
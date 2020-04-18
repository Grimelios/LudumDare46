using System.Collections.Generic;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.UI;
using Engine.View._2D;

namespace LD46.Entities.Core
{
	public class SimpleScene2D : IDynamic
	{
		private List<SimpleEntity2D> entities;

		public SimpleScene2D()
		{
			entities = new List<SimpleEntity2D>();
		}

		public Camera2D Camera { get; set; }
		public Canvas Canvas { get; set; }

		public T Add<T>(T entity) where T : SimpleEntity2D
		{
			entity.Scene = this;
			entities.Add(entity);

			return entity;
		}

		public void Remove(SimpleEntity2D entity)
		{
			entities.Remove(entity);
		}

		public void Update()
		{
			entities.ForEach(e =>
			{
				if (e.IsUpdateEnabled)
				{
					e.Update();
				}
			});
		}

		public void Draw(SpriteBatch sb, float t)
		{
			entities.ForEach(e =>
			{
				if (e.IsUpdateEnabled)
				{
					e.Draw(sb, t);
				}
			});
		}
	}
}

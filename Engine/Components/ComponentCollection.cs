using System.Collections.Generic;
using System.Diagnostics;
using Engine.Interfaces;

namespace Engine.Components
{
	public class ComponentCollection : IDynamic
	{
		private List<IComponent> components = new List<IComponent>();

		// Returning the component allows for chained function calls (if desired).
		public T Add<T>(T component) where T : IComponent
		{
			Debug.Assert(component != null, "Can't add a null component to a collection.");

			components.Add(component);

			return component;
		}

		public void Remove(IComponent component)
		{
			Debug.Assert(components.Contains(component), "The given component isn't part of the collection (was " +
				"either never added or already removed).");

			components.Remove(component);
		}

		public void Update()
		{
			for (int i = components.Count - 1; i >= 0; i--)
			{
				var component = components[i];

				if (component.IsEnabled)
				{
					component.Update();
				}

				// Components can still be marked complete even when disabled (although this will likely be rare).
				if (component.IsComplete)
				{
					components.RemoveAt(i);
				}
			}
		}
	}
}

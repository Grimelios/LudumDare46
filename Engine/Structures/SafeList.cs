using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Engine.Structures
{
	public class SafeList<T> where T : IDisposable
	{
		private List<T> addList;
		private List<T> removeList;
		private List<T> mainList;

		public SafeList()
		{
			addList = new List<T>();
			removeList = new List<T>();
			mainList = new List<T>();
		}

		public void Add(T item)
		{
			Debug.Assert(item != null, "Can't add null to a safe list.");
			Debug.Assert(!addList.Contains(item), "Duplicate item in the safe list's add list (i.e. the item was " +
				"added multiple times before processing changes).");
			Debug.Assert(!mainList.Contains(item), "Duplicate item in the safe list (i.e. the given item is already " +
				"part of the main list).");

			addList.Add(item);
		}

		public void Remove(T item)
		{
			Debug.Assert(item != null, "Can't remove null from a safe list.");
			Debug.Assert(!removeList.Contains(item), "Duplicate item in the safe list's removal list (i.e. the item " +
				"was removed multiple times before processing changes).");
			Debug.Assert(mainList.Contains(item), "Given item isn't in the safe list (meaning it was either never " +
				"added or already removed).");

			removeList.Add(item);
		}

		public List<T> MainList => mainList;

		public void ProcessChanges()
		{
			// Adding first ensures that if an item is added and removed before changes are processed, it will be
			// correctly removed.
			if (addList.Count > 0)
			{
				mainList.AddRange(addList);
				addList.Clear();
			}

			if (removeList.Count > 0)
			{
				foreach (T item in removeList)
				{
					item.Dispose();
					mainList.Remove(item);
				}

				removeList.Clear();
			}
		}
	}
}

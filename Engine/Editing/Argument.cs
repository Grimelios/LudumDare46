using System.Diagnostics;

namespace Engine.Editing
{
	public class Argument
	{
		public Argument(string name, ArgumentTypes type)
		{
			Debug.Assert(!string.IsNullOrEmpty(name), "Terminal argument name can't be null or empty.");

			Name = name;
			Type = type;
		}

		public string Name { get; }

		public ArgumentTypes Type { get; }
	}
}

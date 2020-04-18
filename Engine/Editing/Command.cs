using System.Diagnostics;

namespace Engine.Editing
{
	public abstract class Command
	{
		protected Command(params string[] keywords)
		{
			Debug.Assert(keywords.Length > 0, "Commands must use at least one keyword (should be passed from parent " +
				"constructors).");

			Keywords = keywords;
		}

		public string[] Keywords { get; }

		// By default, commands use no additional arguments (past the keyword itself).
		public virtual Argument[] Usage => null;

		// Many commands return options contextually (based on earlier arguments).
		public virtual string[] GetOptions(string[] args)
		{
			return null;
		}

		public virtual bool Process(string[] args, out string result)
		{
			result = "Not implemented.";

			return false;
		}
	}
}

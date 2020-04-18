using System.Collections.Generic;
using System.Linq;
using Engine.Editing;

namespace Engine.Props.Commands
{
	internal class SetCommand : Command
	{
		private Dictionary<string, string> map;
		private PropertyAccessor accessor;
		private string[] sortedKeys;

		public SetCommand(Dictionary<string, string> map, PropertyAccessor accessor, string[] sortedKeys) : base("set")
		{
			this.map = map;
			this.accessor = accessor;
			this.sortedKeys = sortedKeys;
		}

		public override Argument[] Usage => new[]
		{
			new Argument("property", ArgumentTypes.Required),
			new Argument("value", ArgumentTypes.Required)
		};

		public override string[] GetOptions(string[] args)
		{
			// Property values aren't autocompleted.
			return args.Length == 0 ? sortedKeys : null;
		}

		public override bool Process(string[] args, out string message)
		{
			var key = args[0];

			if (!map.ContainsKey(key))
			{
				message = $"Unknown property '{key}'.";

				return false;
			}

			var tracker = accessor.Tracker;
			var matchedProperty = tracker.Keys.FirstOrDefault(prop => prop.Key == key);

			if (matchedProperty.Key == null)
			{
				message = $"Property '{key}' is untracked.";

				return false;
			}

			var targets = tracker[matchedProperty];
			var value = args[1];
			var oldValue = map[key];

			if (value == oldValue)
			{
				message = "Property unchanged.";

				return true;
			}

			map[key] = value;

			foreach (var target in targets)
			{
				// In nearly all cases, a property will only be used by a single class (although there might be
				// multiple instances of that class, e.g. multiple enemies using the same property). As such,
				// validating that property on each reload is technically wasteful, but it's not worth the effort to
				// attempt to avoid that repetition (it'd complicate the interface).
				if (target.Reload(accessor, out message))
				{
					map[key] = oldValue;

					return false;
				}
			}

			message = $"Property '{key}' modified.";

			return true;
		}
	}
}

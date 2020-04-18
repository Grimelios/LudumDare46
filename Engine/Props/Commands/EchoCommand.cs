using System.Collections.Generic;
using Engine.Editing;

namespace Engine.Props.Commands
{
	internal class EchoCommand : Command
	{
		private Dictionary<string, string> map;
		private string[] sortedKeys;

		public EchoCommand(Dictionary<string, string> map, string[] sortedKeys) : base("echo")
		{
			this.map = map;
			this.sortedKeys = sortedKeys;
		}

		public override Argument[] Usage => new []
		{
			new Argument("property", ArgumentTypes.Required)
		};

		public override string[] GetOptions(string[] args)
		{
			return sortedKeys;
		}

		public override bool Process(string[] args, out string result)
		{
			var key = args[0];

			if (!map.TryGetValue(key, out result))
			{
				result = $"Unknown property '{key}'.";

				return false;
			}

			return true;
		}
	}
}

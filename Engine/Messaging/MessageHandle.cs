using System;

namespace Engine.Messaging
{
	public class MessageHandle
	{
		internal MessageHandle(int type, Action<object> function)
		{
			Type = type;
			Function = function;
		}

		internal int Type { get; }
		internal Action<object> Function { get; }
	}
}

using System;
using System.Collections.Generic;

namespace Engine.Messaging
{
	public interface IReceiver : IDisposable
	{
		List<MessageHandle> MessageHandles { get; set; }
	}
}

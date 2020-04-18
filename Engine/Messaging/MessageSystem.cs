using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine.Messaging
{
	public static class MessageSystem
	{
		private static Dictionary<int, HashSet<Action<object>>> functionMap;
		private static List<(IReceiver Receiver, int MessageType, Action<object> Function)> addList;
		private static List<(IReceiver Receiver, int MessageType)> removeList;

		// Receivers can't be directly added or removed from callbacks since you can't modify a list mid-loop. The
		// usual looping backwards trick isn't safe either since the destruction of one object might remove multiple
		// receivers (not just the item itself).
		private static bool isSendActive;

		static MessageSystem()
		{
			functionMap = new Dictionary<int, HashSet<Action<object>>>();
			addList = new List<(IReceiver Receiver, int MessageType, Action<object> Function)>();
			removeList = new List<(IReceiver Receiver, int MessageType)>();
		}

		public static void Subscribe(IReceiver receiver, int messageType, Action<object> function)
		{
			Debug.Assert(receiver != null, "Receiver can't be null.");
			Debug.Assert(function != null, "Receiver function can't be null.");
			Debug.Assert(messageType >= 0, "When subscribing, message type can't be negative.");

			// Initializing the handle list here simplifies contructors for receiving classes (since they don't all
			// need to individually create those lists themselves).
			if (receiver.MessageHandles == null)
			{
				receiver.MessageHandles = new List<MessageHandle>();
			}

			Debug.Assert(receiver.MessageHandles.All(h => h.Type != messageType), "The given receiver is already " +
				$"subscribed to message type {messageType}.");

			if (isSendActive)
			{
				addList.Add((receiver, messageType, function));
			}
			else
			{
				Add(receiver, messageType, function);
			}
		}

		private static void Add(IReceiver receiver, int messageType, Action<object> function)
		{
			if (!functionMap.TryGetValue(messageType, out var functions))
			{
				functions = new HashSet<Action<object>>();
				functionMap.Add(messageType, functions);
			}

			functions.Add(function);
			receiver.MessageHandles.Add(new MessageHandle(messageType, function));
		}

		public static void Unsubscribe(IReceiver receiver, int messageType = -1)
		{
			Debug.Assert(receiver != null, "Receiver can't be null.");
			Debug.Assert(messageType >= -1, "When unsubscribing, message type must be either non-negative (to " +
				"unsubscribe a single message type) or -1 (to unsubscribe from all message types).");
			Debug.Assert(messageType == -1 || receiver.MessageHandles.Any(h => h.Type == messageType), "The given " +
				$"receiver isn't subscribed to message type {messageType}.");

			if (isSendActive)
			{
				removeList.Add((receiver, messageType));
			}
			else
			{
				Remove(receiver, messageType);
			}
		}

		private static void Remove(IReceiver receiver, int messageType)
		{
			var handles = receiver.MessageHandles;

			// Passing -1 unsubscribes the receiver from all message types. Passing a message type explicitly
			// unsubscribes from only that type.
			if (messageType != -1)
			{
				foreach (var handle in handles)
				{
					functionMap[handle.Type].Remove(handle.Function);
				}

				handles.Clear();

				return;
			}

			for (int i = 0; i < handles.Count; i++)
			{
				var handle = handles[i];

				if (handle.Type == messageType)
				{
					functionMap[messageType].Remove(handle.Function);
					handles.RemoveAt(i);

					return;
				}
			}
		}

		public static void Send(int messageType, object data = null)
		{
			Debug.Assert(messageType >= 0, "Sent message type can't be negative.");

			// This means that no receivers have subscribed to the given message type.
			if (!functionMap.TryGetValue(messageType, out var set))
			{
				return;
			}

			isSendActive = true;

			foreach (var function in set)
			{
				function(data);
			}

			isSendActive = false;
			ProcessChanges();
		}

		private static void ProcessChanges()
		{
			if (addList.Count > 0)
			{
				foreach (var item in addList)
				{
					Add(item.Receiver, item.MessageType, item.Function);
				}

				addList.Clear();
			}

			if (removeList.Count > 0)
			{
				foreach (var item in removeList)
				{
					Remove(item.Receiver, item.MessageType);
				}

				removeList.Clear();
			}
		}
	}
}

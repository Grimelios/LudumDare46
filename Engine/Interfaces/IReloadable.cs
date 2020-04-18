using System;
using Engine.Props;

namespace Engine.Interfaces
{
	public interface IReloadable : IDisposable
	{
		bool Reload(PropertyAccessor accessor, out string message);
	}
}

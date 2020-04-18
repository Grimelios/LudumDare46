using System;
using Engine.Input.Data;

namespace Engine.Input
{
	public interface IControllable : IDisposable
	{
		InputFlowTypes ProcessInput(FullInputData data);
	}
}

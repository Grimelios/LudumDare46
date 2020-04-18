using System;

namespace Engine.Graphics
{
	[Flags]
	public enum RenderTargetFlags
	{
		None = 0,
		Color = 1,
		Depth = 2,
		DepthStencil = Depth | Stencil,
		Stencil = 4,
	}
}

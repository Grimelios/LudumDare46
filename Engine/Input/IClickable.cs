using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.Input
{
	// TODO: Support rotation as well (if needed).
	public interface IClickable : IPositionContainer2D
	{
		bool IsEnabled { get; set; }

		// TODO: For high-DPI mouse settings, might have to make these regular vec2s.
		void OnHover(ivec2 mouseLocation);
		void OnUnhover();
		void OnClick(ivec2 mouseLocation);
		void OnRelease();

		bool Contains(ivec2 mouseLocation);
	}
}

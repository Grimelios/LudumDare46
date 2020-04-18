using static Engine.Graphics.GL;

namespace Engine.Graphics
{
	public enum BufferFrequencies
	{
		Dynamic = GL_DYNAMIC_DRAW,
		Static = GL_STATIC_DRAW,
		Stream = GL_STREAM_DRAW
	}
}

using static Engine.Graphics.GL;

namespace Engine.Shaders
{
	public enum ShaderTypes
	{
		Fragment = GL_FRAGMENT_SHADER,
		Geometry = GL_GEOMETRY_SHADER,
		TessellationControl = GL_TESS_CONTROL_SHADER,
		TessellationEvaluation = GL_TESS_EVALUATION_SHADER,
		Vertex = GL_VERTEX_SHADER
	}
}

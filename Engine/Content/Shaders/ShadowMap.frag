#version 440 core

const int MaxMaterials = 6;

in vec2 fSource;
in flat float fSourceIndex;

uniform sampler2D images[MaxMaterials];

void main()
{
	vec4 color = texture(images[int(fSourceIndex)], fSource);

	// Only non-transparent pixels cast shadows.
	gl_FragDepth = color.a == 0 ? 1 : gl_FragCoord.z;
}

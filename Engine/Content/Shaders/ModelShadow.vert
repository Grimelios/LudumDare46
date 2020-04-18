#version 440 core

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vSource;
layout (location = 2) in float vSourceIndex;
layout (location = 3) in vec3 vNormal;

out vec3 fPosition;
out vec2 fSource;
out flat float fSourceIndex;
out vec4 fShadowMapCoords;
out vec3 fNormal;

uniform mat4 orientation;
uniform mat4 worldMatrix;
uniform mat4 vpMatrix;
uniform mat4 lightBiasMatrix;

void main()
{
	mat4 mvp = vpMatrix * worldMatrix;
	vec4 position = vec4(vPosition, 1);

	gl_Position = mvp * position;
	
	fPosition = (worldMatrix * position).xyz;
	fSource = vSource;
	fSourceIndex = vSourceIndex;
	fNormal = (orientation * vec4(vNormal, 1)).xyz;
	fShadowMapCoords = lightBiasMatrix * position;
}

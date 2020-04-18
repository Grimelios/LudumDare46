#version 440 core

const int MaxBones = 101;

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec2 vSource;
layout (location = 2) in float vSourceIndex;
layout (location = 3) in vec3 vNormal;
layout (location = 4) in vec2 boneWeights;
layout (location = 5) in vec2 boneIndexes;

out vec3 fPosition;
out vec2 fSource;
out flat float fSourceIndex;
out vec3 fNormal;
out vec4 fShadowMapCoords;

uniform mat4 orientation;
uniform mat4 worldMatrix;
uniform mat4 vpMatrix;
uniform mat4 lightBiasMatrix;
uniform vec3 poseOrigin;
uniform vec3 defaultPose[MaxBones];
uniform vec3 bonePositions[MaxBones];
uniform vec4 boneOrientations[MaxBones];

vec3 quatMultiply(vec4 q, vec3 v) {
	// See https://community.khronos.org/t/quaternion-functions-for-glsl/50140/3.
	return v + 2 * cross(cross(v, q.xyz ) + q.w * v, q.xyz);
} 

void main()
{
	vec4 position = vec4(vPosition, 1);
	vec4 normal = vec4(vNormal, 1);
	
	float w1 = boneWeights.x;
	float w2 = boneWeights.y;

	int index1 = int(boneIndexes.x);
	int index2 = int(boneIndexes.y);

	vec4 q1 = boneOrientations[index1];
	vec4 q2 = boneOrientations[index2];

	vec3 pose1 = defaultPose[index1];
	vec3 pose2 = defaultPose[index2];
	vec3 localPosition1 = (vPosition + poseOrigin) - pose1;
	vec3 localPosition2 = (vPosition + poseOrigin) - pose2;
	vec3 rotated1 = (quatMultiply(q1, localPosition1) - localPosition1) * w1;
	vec3 rotated2 = (quatMultiply(q2, localPosition2) - localPosition2) * w2;
	vec3 localBone1 = bonePositions[index1] - pose1;
	vec3 localBone2 = bonePositions[index2] - pose2;

	position.xyz += poseOrigin + localBone1 * w1 + localBone2 * w2 + rotated1 + rotated2;
	
	vec3 n1 = (quatMultiply(q1, vNormal) - vNormal) * w1;
	vec3 n2 = (quatMultiply(q2, vNormal) - vNormal) * w2;

	normal.xyz += n1 + n2;
	
	mat4 mvp = vpMatrix * worldMatrix;

	gl_Position = mvp * position;

	fPosition = (worldMatrix * position).xyz;
	fSource = vSource;
	fSourceIndex = vSourceIndex;

	// TODO: Does this need to be normalized? It's not normalized in ModelShadow.vert.
	fNormal = normalize((orientation * normal).xyz);
	fShadowMapCoords = lightBiasMatrix * position;
}

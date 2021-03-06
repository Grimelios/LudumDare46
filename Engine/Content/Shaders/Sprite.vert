﻿#version 440 core

layout (location = 0) in vec2 vPosition;
layout (location = 1) in vec2 vSource;
layout (location = 2) in vec4 vColor;

out vec2 fSource;
out vec4 fColor;

uniform mat4 mvp;

void main()
{
	vec4 position = mvp * vec4(vPosition, 0, 1);
	position.y *= -1;

	gl_Position = position;

	fSource = vSource;
	fColor = vColor;
}

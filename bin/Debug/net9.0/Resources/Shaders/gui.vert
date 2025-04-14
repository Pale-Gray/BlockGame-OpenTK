#version 460 core

layout (location=0) in vec2 position;
layout (location=1) in vec2 textureCoordinates;
layout (location=2) in float layer;

layout (location=0) uniform mat4 view;
layout (location=1) uniform mat4 projection;

void main()
{

	gl_Position = vec4(position, -1.0 + -((10000.0 - layer) / 10000.0), 1.0) * view * projection;

}
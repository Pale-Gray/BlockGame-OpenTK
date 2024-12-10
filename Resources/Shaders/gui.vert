#version 400 core
layout (location=0) in vec2 position;
layout (location=1) in vec2 textureCoordinates;
layout (location=2) in int layer;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec2 guiSize;
uniform int textureMode;

out vec2 vTextureCoordinates;
out vec2 vGuiSpacePosition;

void main()
{

	vGuiSpacePosition = position;

	gl_Position = vec4(position, layer, 1.0) * view * projection;

	vTextureCoordinates = textureCoordinates;

}
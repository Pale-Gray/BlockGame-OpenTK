#version 460 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 textureCoordinate;
layout (location = 2) in vec3 glyphColor;
layout (location = 3) in float glyphTextureIndex;

uniform mat4 view;
uniform mat4 projection;

out vec2 vTextureCoordinate;
out vec3 vGlyphColor;
out float vGlyphTextureIndex;

void main() 
{

	gl_Position = vec4(position, 1.0) * view * projection;

	vTextureCoordinate = textureCoordinate;
	vGlyphColor = glyphColor;
	vGlyphTextureIndex = glyphTextureIndex;

}


#version 460 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 textureCoordinate;
layout (location = 2) in vec4 glyphColor;
layout (location = 3) in float glyphTextureIndex;

uniform mat4 view;
uniform mat4 projection;

out vec2 vTextureCoordinate;
out vec4 vGlyphColor;
out float vGlyphTextureIndex;

void main() 
{

	gl_Position = vec4(position.xy, -1.0 + -((10000.0 - position.z) / 10000.0), 1.0) * view * projection;

	vTextureCoordinate = textureCoordinate;
	vGlyphColor = glyphColor;
	vGlyphTextureIndex = glyphTextureIndex;

}


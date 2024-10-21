#version 460 core
#extension GL_ARB_gpu_shader_int64: require

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 textureCoordinate;
layout (location = 2) in uvec2 textureHandle;

uniform mat4 view;
uniform mat4 projection;

out uvec2 vTextureHandle;
out vec2 vTextureCoordinate;

void main() 
{

	gl_Position = vec4(position, 1.0) * view * projection;

	vTextureHandle = textureHandle;
	vTextureCoordinate = textureCoordinate;

}


#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 texcoord;

out vec3 v_position;
out vec2 v_texcoord;

uniform mat4 transformation;

void main()
{

	v_position = position;
	v_texcoord = texcoord;

	gl_Position = vec4(position, 1.0) * transformation;

}
#version 330 core
layout (location=0) in vec3 position;

out vec3 v_position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{

	v_position = position;

	gl_Position = vec4(position.xyz, 1.0) * model * view * projection;

}
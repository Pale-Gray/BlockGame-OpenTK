#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 texcoord;

out vec3 v_position;
out vec2 v_texcoord;

out float v_time;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

float PI = 3.141459265359;

uniform float time;

void main()
{

	v_position = position;
	v_texcoord = texcoord;
	v_time = time;

	gl_Position = vec4(position.xyz, 1.0) * model * view * projection;

}
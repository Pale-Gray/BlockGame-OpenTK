#version 330 core
layout (location=0) in float blocktype;
layout (location=1) in vec3 position;
layout (location=2) in vec3 normal;
layout (location=3) in vec2 texcoord;

out vec3 v_position;
out vec3 v_normal;
out vec2 v_texcoord;
out float v_blocktype;
out vec3 c_position;
out float v_time;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 cpos;

float PI = 3.141459265359;

uniform float time;

void main()
{

	v_position = position;
	v_normal = normal;
	v_texcoord = texcoord;
	v_blocktype = blocktype;
	v_time = time;

	c_position = cpos;

	gl_Position = vec4(position.xyz, 1.0) * model * view * projection;

}
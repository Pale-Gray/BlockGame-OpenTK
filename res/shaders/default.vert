#version 330 core
layout (location=0) in float blocktype;
layout (location=1) in vec3 position;
layout (location=2) in vec3 normal;
layout (location=3) in vec2 texcoord;

out vec3 v_position;
out vec3 v_normal;
out vec2 v_texcoord;
out float v_blocktype;

out float v_time;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float time;

void main()
{

	v_position = position;
	v_normal = normal;
	v_texcoord = texcoord;
	v_blocktype = blocktype;
	v_time = time;

	gl_Position = vec4(position.x, position.y, position.z, 1.0) * model * view * projection;

}
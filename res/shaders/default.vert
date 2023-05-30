#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 texcoord;
layout (location=2) in float blocktype;

out vec3 v_position;
out vec2 v_texcoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float time;

void main()
{

	v_position = position;
	v_texcoord = texcoord;

	if (blocktype == 1) {

		gl_Position = vec4(position.x,
	position.y + (sin(position.x+time)/5),
	position.z + (sin(position.x+time)/5),
	1.0) * model * view * projection;

	} else {

	gl_Position = vec4(position.x,
	position.y,
	position.z,
	1.0) * model * view * projection;

	}

}
#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 texcoord;

uniform float time;

out vec3 v_position;
out vec2 v_texcoord;
out float v_time;

void main()
{

	v_position = position;
	v_texcoord = texcoord;
	v_time = time;

	gl_Position = vec4(position.xy, 0.0, 1.0);

}
#version 330 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;

in float v_time;

uniform sampler2D tex;

vec3 color = vec3(0,0,0);

void main()
{

	vec4 tex = (texture(tex, v_texcoord).rgba); // * (intensity + 0.25);

	v_position.x > 0 ? color.r = 1 : color.r = 0;
	v_position.y > 0 ? color.g = 1 : color.g = 0;
	v_position.z > 0 ? color.b = 1 : color.b = 0;

	FragColor = vec4(color.rgb, 1.0);

}
#version 330 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;
in vec3 v_normal;
in float v_blocktype;

in float v_time;

uniform sampler2D tex;

void main()
{
	
	float lighting = 1;

	// fake ass lighting temporary though
	if (v_normal.y == 1 || v_normal.y == -1) { lighting = 1.5; }
	if (v_normal.x == 1 || v_normal.x == -1) { lighting = 0.9; }
	if (v_normal.z == 1 || v_normal.z == -1) { lighting = 1; }

	vec4 tex = (texture(tex, v_texcoord).rgba); // * (intensity + 0.25);

	float ambient = 0.5;

	vec3 lightdirection = vec3(sin(v_time), cos(v_time), 0.0);
	float brightness = max(ambient, dot(lightdirection, v_normal));

	FragColor = tex.rgba * brightness;

}
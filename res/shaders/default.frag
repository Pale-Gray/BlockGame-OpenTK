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

	vec3 lightnormal = vec3(0.8, 1, 0.6);
	float brightness = max(ambient, dot(lightnormal, v_normal));

	FragColor = vec4(tex.rgb * brightness, tex.a);

}
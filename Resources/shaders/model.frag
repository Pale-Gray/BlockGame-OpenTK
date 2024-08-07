#version 330 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;

in float v_time;

uniform sampler2D tex;

void main()
{

	vec4 tex = (texture(tex, v_texcoord).rgba); // * (intensity + 0.25);

	vec3 color1 = vec3(155.0/255, 197.0/255, 237.0/255);
	vec3 color2 = vec3(13.0/255, 15.0/255, 48.0/255);

	// FragColor = vec4(mix(color1*1.2, color1*1.4, v_position.y*1.2),1);//tex.rgba;
	vec3 topColor = vec3(0.251,0.227,0.945);
	vec3 bottomColor = vec3(0.522,0.667,0.933);

	FragColor = vec4(v_texcoord, 0, 1);
	// FragColor = vec4(0,0,0,1);
}
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

	// FragColor = vec4(1.0, 1.0, 0.0, 1.0);
	vec3 nnormal = normalize(v_normal);
	vec3 lightdir = vec3(cos(v_time),1,sin(v_time));

	float intensity = max(0.0, dot(nnormal, lightdir));

	vec4 tex = (texture(tex, v_texcoord).rgba); // * (intensity + 0.25);

	FragColor = tex;

}
#version 330 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;

uniform sampler2D tex;

void main()
{

	// FragColor = vec4(1.0, 1.0, 0.0, 1.0);

	vec4 tex = texture(tex, v_texcoord).rgba;

	FragColor = tex;

}
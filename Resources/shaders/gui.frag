#version 400 core
out vec4 FragColor;

in vec2 v_TexCoords;

uniform sampler2D tex;

void main()
{

	// FragColor = vec4(1.0, 0.0, 0.0, 1.0);
	FragColor = texture(tex, v_TexCoords);

}
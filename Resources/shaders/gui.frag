#version 400 core
out vec4 FragColor;

in vec2 v_TexCoords;

uniform sampler2D tex;
uniform vec3 colorTint;

void main()
{

	// FragColor = vec4(1.0, 0.0, 0.0, 1.0);
	FragColor = vec4(1,1,1,1) * vec4(colorTint, 1);
	// FragColor = vec4(0,0,0,1);

}
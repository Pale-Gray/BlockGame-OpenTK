#version 400 core
out vec4 FragColor;

in vec2 vtextureCoordinates;
uniform sampler2D fatlas;

uniform vec3 fontColor;

void main()
{

	// FragColor = vec4(1.0, 1.0, 0.0, 1.0);
	FragColor = texture(fatlas, vtextureCoordinates) * vec4(fontColor, 1.0);
	// FragColor = texture(tex, v_TexCoords);

}
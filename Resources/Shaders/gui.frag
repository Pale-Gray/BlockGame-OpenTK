#version 400 core
out vec4 FragColor;

in vec2 vTextureCoordinates;

uniform sampler2D guiTexture;
uniform vec3 guiColor;

void main()
{

	// FragColor = vec4(vTextureCoordinates, 0.0, 1.0);
	vec4 guiTex = texture(guiTexture, vTextureCoordinates);
	FragColor = vec4(guiTex.rgb * guiColor, guiTex.a);

}
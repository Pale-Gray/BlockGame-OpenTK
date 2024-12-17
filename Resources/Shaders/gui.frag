#version 460 core
out vec4 FragColor;

float manhattan_distance(vec2 a, vec2 b) {

	return abs(a.x-b.x) + abs(a.y-b.y);

}

layout (location=2) uniform vec4 color;

void main()
{

	// vec4 guiTex = texture(guiTexture, vTextureCoordinates);
	// FragColor = vec4(guiTex.rgb * guiColor, guiTex.a);

	FragColor = vec4(color);
	
}
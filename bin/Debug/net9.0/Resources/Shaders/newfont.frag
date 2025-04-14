#version 400 core
out vec4 FragColor;

uniform sampler2D fontTexture;

in vec2 vTexCoord;

void main()
{

	vec4 fontColor = texture(fontTexture, vec2(vTexCoord.x, vTexCoord.y));

	float color = fontColor.r;

	FragColor = vec4(1, 1, 1, color);

}
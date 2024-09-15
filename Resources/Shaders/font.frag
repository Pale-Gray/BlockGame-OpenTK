#version 400 core
out vec4 FragColor;

uniform sampler2D fontTexture;

in vec2 vTexCoord;
in vec3 vColor;

void main()
{

	vec4 fontColor = texture(fontTexture, vTexCoord);

	if (fontColor.a == 0.0) discard;

	FragColor = vec4(fontColor.rgb * vColor, fontColor.a);

}
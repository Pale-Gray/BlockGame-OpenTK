#version 400 core
out vec4 FragColor;

uniform sampler2D atlas;

in vec3 vposition;
in vec2 vtexcoord;
in int vblocktype;
in float vtime;
in vec3 vnormal;
in float vambientValue;

void main()
{
	// textureSize(tex,0);
	vec3 directionalLight = vec3(0.3, -0.7, 0.5);

	// FragColor = textureCube(cubemap, R);
	float ambient = 0.2;
	float value = ambient + max(0, dot(directionalLight, -vnormal));

	// FragColor = vec4(texture(atlas, vtexcoord).rgb * value, 1.0);
	vec4 tex = texture(atlas, vtexcoord);
	FragColor = vec4(tex.rgb * value, 1.0);
	// FragColor = vec4(v_normal, 1);

}
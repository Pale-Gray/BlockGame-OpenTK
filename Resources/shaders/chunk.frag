#version 400 core
out vec4 FragColor;

uniform sampler2D atlas;

in vec3 vposition;
in vec2 vtexcoord;
in int vblocktype;
in float vtime;
in vec3 vnormal;
in float vambientValue;

in vec3 directionalLight;

vec2[] texcoord = vec2[](vec2(0,0), vec2(1,0), vec2(1,1), vec2(0,1));

void main()
{
	// textureSize(tex,0);

	// FragColor = textureCube(cubemap, R);
	float ambient = 0.2;
	float value = ambient + max(0, dot(directionalLight, vnormal));

	// FragColor = vec4(texture(atlas, vtexcoord).rgb * value, 1.0);
	vec4 tex = texture(atlas, vtexcoord);
	float edge = 0;
	if (vposition.x > 0.1 && vposition.x < 31.9 && vposition.z > 0.1 && vposition.z < 31.9) {edge = 1;}

	FragColor = vec4((tex.rgb * value) * edge, 1.0);
	// FragColor = vec4(1,1,1, 1.0);

	// FragColor = vec4(v_normal, 1);

}
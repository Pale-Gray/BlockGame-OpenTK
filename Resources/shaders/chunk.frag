#version 400 core
// out vec4 Outcolor;

uniform sampler2D atlas;
uniform sampler2DArray arrays;

in vec3 vposition;
in vec2 vtexcoord;
flat in int vtexture_index;
in float vtime;
in vec3 vnormal;
in float vambient_value;

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
	vec4 array_texture = texture(arrays, vec3(vtexcoord, vtexture_index));
	float edge = 0;
	float thickness = 0.1;
	if (vposition.x > thickness && vposition.x < 32 - thickness && vposition.y > thickness && vposition.y < 32 - thickness && vposition.z > thickness && vposition.z < 32 - thickness) {edge = 1;}

	// Outcolor = vec4((array_texture.rgb) * value * vambient_value, 1.0);
	
	gl_FragColor = vec4((array_texture.rgb) * value * vambient_value, 1.0);

}
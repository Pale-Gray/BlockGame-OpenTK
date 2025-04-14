#version 400 core
out vec4 Outcolor;

uniform sampler2D atlas;
uniform sampler2DArray arrays;
uniform vec3 cameraPosition;

uniform float radius;

uniform mat4 view;
uniform mat4 projection;

in vec3 vposition;
in vec2 vtexcoord;
flat in int vtexture_index;
in float vtime;
in vec3 vnormal;
in float vambient_value;

in vec3 directionalLight;

in vec3 vPositionOffset;

in float distFac;

uniform bool shouldRenderFog;
uniform float fogOffset;

uniform float chunkLifetime;
uniform bool shouldRenderAmbientOcclusion;

in vec4 ambientValues;

vec2[] texcoord = vec2[](vec2(0,0), vec2(1,0), vec2(1,1), vec2(0,1));

float dist3D(vec3 pos1, vec3 pos2) 
{

	float x = pow(pos2.x - pos1.x, 2);
	float y = pow(pos2.y - pos1.y, 2);
	float z = pow(pos2.z - pos1.z, 2);
	return sqrt(x+y+z);

}

void main()
{

	vec3 normal = gl_FrontFacing ? vnormal : -vnormal;

	float sunDotProduct = dot(directionalLight, normal);

	float falloff = dot(vec3(0,1,0), directionalLight);
	float ambient = clamp(0.5, 0, 1);
	float value = clamp(ambient + (max(0, sunDotProduct)), 0.1, 1.0);

	vec4 tex = texture(atlas, vtexcoord);
	float a = texelFetch(arrays, ivec3(vtexcoord * 32.0, vtexture_index), 0).a;
	vec4 array_texture = texture(arrays, vec3(vtexcoord, vtexture_index));
	if (a == 0) discard;
	Outcolor = vec4(array_texture.rgb * value, 1);

}
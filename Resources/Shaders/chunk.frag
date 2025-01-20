#version 460 core

precision mediump float;
precision mediump sampler2DArray;

out vec4 Outcolor;

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
flat in uint vlight_data;

in vec3 vLightColor;
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

	float sunDotProduct = dot(-directionalLight, normal);

	float falloff = dot(vec3(0,1,0), -directionalLight);
	float ambient = clamp(0.5 * falloff, 0, 1);
	float value = clamp(ambient + (max(0, sunDotProduct) * falloff), 0.1, 1.0);
	// float value = sunDotProduct;

	vec4 array_texture = texture(arrays, vec3(vtexcoord, vtexture_index));
	// float edge = 0;
	float thickness = 0.1;
	// if (vposition.x > thickness && vposition.x < 32 - thickness && vposition.y > thickness && vposition.y < 32 - thickness && vposition.z > thickness && vposition.z < 32 - thickness) {edge = 1;}

	vec3 fogColor = vec3(0.522,0.667,0.933);

	float fac = clamp(distFac, 0, 1);

	float a = 1.0;
	// float texelAlpha = texelFetch(arrays, ivec3(vtexcoord*32,vtexture_index), 0).a;
	float texelAlpha = array_texture.a;
	if (texelAlpha == 0) discard;

	float ambientIntensity = 0.5;
	float falloffMultiplier = 0.0;

	float ambientValue = ambientValues.r;
	// if (ambientValue == 0.0) ambientValue = ambientIntensity;

	vec4 ambientOcclusion = vec4(ambientValue);
	if (!shouldRenderAmbientOcclusion)
	{
		ambientOcclusion = vec4(1,1,1,1);
	}

	float ambientFac = (ambientIntensity + falloffMultiplier) * (ambientIntensity * (1-ambientOcclusion.r)) - (ambientIntensity * falloffMultiplier);
	ambientFac = clamp(ambientFac, 0, 1);

// 	Outcolor = vec4(array_texture.rgb * (1 - (vambient_value/3)), 1);
	// ambientValue = mix(-0.1, 1.0, (1 - (vambient_value/4)));
	ambientValue = 1.0 - (vambient_value / 4.0);

	vec3 lightColor = vec3((vlight_data >> 12) & 15,(vlight_data >> 8) & 15,(vlight_data >> 4) & 15) / 15.0;
	vec3 lightValue = lightColor;
	vec3 sunLight = vec3(vlight_data & 15) / 15.0;

	// vec3 col = vec3((array_texture.rgb * ambientValue * value) * (pow(1 + lightValue, vec3(2))));
	
	// vec3 col = vec3((array_texture.rgb * pow(1 + lightValue, vec3(2)))) * value * (pow(ambientValue, 1.2));

	vec3 col = 1.0 * (vec3(0.0) + vLightColor);
	
	Outcolor = vec4(col, a);
	// Outcolor = clamp(vec4(vnormal, 1.0) * inverse(transpose(view)), 0.0, 1.0);

}
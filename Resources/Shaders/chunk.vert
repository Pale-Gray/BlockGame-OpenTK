#version 460 core

struct PackedVec3
{
	float x, y, z;
};

struct PackedVec2
{
	float x, y;
};

struct ChunkVertex
{
	PackedVec3 position;
	int ID;
	float ambientValue;
	bool shouldRenderAO;
	int textureIndex;
	uint lightData;
	PackedVec2 texcoords;
	PackedVec3 normal;
}; 

layout (std430, binding=3) readonly buffer chunkData
{
	ChunkVertex vertexData[];
};

uniform mat4 view;
uniform mat4 projection;
uniform float time;
uniform vec3 cameraPosition;

uniform mat4 rot;
uniform vec3 chunkpos;
uniform vec3 sunDirection;

out vec3 vposition;
out vec3 vchunkposition;
out vec2 vtexcoord;
flat out int vtexture_index;
out vec3 vnormal;
out float vtime;
out float vambient_value;
out uint vlight_data;

out float distFac;
uniform float chunkLifetime;
out vec3 directionalLight;
uniform float radius;
out vec3 vPositionOffset;
out vec4 ambientValues;

out vec3 vLightColor;

float dist3D(vec3 pos1, vec3 pos2) 
{

	float x = pow(pos2.x - pos1.x, 2);
	float y = pow(pos2.y - pos1.y, 2);
	float z = pow(pos2.z - pos1.z, 2);
	return sqrt(x+y+z);

}

vec3 unpack(PackedVec3 vector)
{
	return vec3(vector.x, vector.y, vector.z);
}

vec2 unpack(PackedVec2 vector) 
{
	return vec2(vector.x, vector.y);
}

vec3 getLightData(uint light_data) {
	
	return vec3((light_data >> 12) & 15, (light_data >> 8) & 15, (light_data) >> 4 & 15) / 15.0;
	
}

void main()
{

	vec3 position = unpack(vertexData[gl_VertexID].position);
	vec2 texcoord = unpack(vertexData[gl_VertexID].texcoords);
	int texture_index = vertexData[gl_VertexID].textureIndex;
	vec3 normal = unpack(vertexData[gl_VertexID].normal);
	float ambient_value = vertexData[gl_VertexID].ambientValue;
	uint light_data = vertexData[gl_VertexID].lightData;
	
	vLightColor = getLightData(vertexData[gl_VertexID].lightData);
	
	vposition = position;
	vtexcoord = texcoord;
	vtexture_index = texture_index;
	vtime = time;
	vnormal = normal;
	vambient_value = ambient_value;
	vPositionOffset = position + (chunkpos * 32);

	ambientValues = vec4(ambient_value, ambient_value, ambient_value, 1.0);
	// if (ambient_value == 0.0) ambientValues.rgb = vec3(0.65);

	// directionalLight = sunDirection;
	directionalLight = vec3(-0.5, -1, 0.25);
	vlight_data = light_data;

	distFac = clamp(dist3D(vPositionOffset, cameraPosition) / (radius*32), 0, 1);

	// float fac = 2;

	//  - max((pow(1 - chunkLifetime, 15)), 0)
	float t = min(chunkLifetime, 1);
	float displace = pow(1 - t, 15);


	float displacement = dist3D(position + (chunkpos*32), cameraPosition);
	// clamp(displace, 0, displace);
	// float displace = min(32 * (chunkLifetime/5), 0);

	gl_Position = vec4(vec3(position.x, position.y, position.z) + (vec3(chunkpos.x, chunkpos.y, chunkpos.z) * 32), 1.0) * view * projection;
	// gl_Position = vec4(vertexData[gl_VertexID].position, 1.0) * view * projection;
	// vec3 pos = vec3(vertexData[gl_VertexID].position[0], vertexData[gl_VertexID].position[1], vertexData[gl_VertexID].position[2]);
	// vec3 pos = vec3(vertexData[gl_VertexID].position.x, vertexData[gl_VertexID].position.y, vertexData[gl_VertexID].position.z);

	// gl_Position = vec4(pos + (vec3(chunkpos) * 32), 1.0) * view * projection;

}
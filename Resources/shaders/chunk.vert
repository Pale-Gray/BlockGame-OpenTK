#version 400 core
layout (location=0) in int texture_index;
layout (location=1) in vec3 position;
layout (location=2) in vec2 texcoord;
layout (location=3) in vec3 normal;
layout (location=4) in float ambient_value;
// layout (location=0) in float blocktype;
// layout (location=1) in vec3 position;
// layout (location=2) in vec3 normal;
// layout (location=3) in vec2 texcoord;

uniform mat4 view;
uniform mat4 projection;
uniform float time;
uniform vec3 cameraPosition;

uniform mat4 rot;
uniform vec3 chunkpos;

out vec3 vposition;
out vec3 vchunkposition;
out vec2 vtexcoord;
flat out int vtexture_index;
out vec3 vnormal;
out float vtime;
out float vambient_value;
// out float vambientValue;

out vec3 directionalLight;

float dist3D(vec3 pos1, vec3 pos2) 
{

	float x = pow(pos2.x - pos1.x, 2);
	float y = pow(pos2.y - pos1.y, 2);
	float z = pow(pos2.z - pos1.z, 2);
	return sqrt(x+y+z);

}

void main()
{
	
	vposition = position;
	vtexcoord = texcoord;
	vtexture_index = texture_index;
	vtime = time;
	vnormal = normal;
	vambient_value = ambient_value;

	directionalLight = normalize((vec4(0,1,0,1))).xyz;

	// vec3 worldPosition = (vec4(position, 1.0) * model).xyz;

	// vec3 coordinates = (vec4(1.0, 1.0, 1.0, 1.0) * model).xyz;

	float fac = 2;

	gl_Position = vec4(position.xyz + (chunkpos * 32), 1.0) * view * projection;

}
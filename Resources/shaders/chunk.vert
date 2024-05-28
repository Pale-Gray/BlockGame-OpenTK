#version 400 core
layout (location=0) in int blocktype;
layout (location=1) in vec3 position;
layout (location=2) in vec2 texcoord;
layout (location=3) in vec3 normal;
layout (location=4) in float ambientValue;
// layout (location=0) in float blocktype;
// layout (location=1) in vec3 position;
// layout (location=2) in vec3 normal;
// layout (location=3) in vec2 texcoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float time;
uniform vec3 cameraPosition;

uniform mat4 rot;

out vec3 vposition;
out vec2 vtexcoord;
out int vblocktype;
out vec3 vnormal;
out float vtime;
out float vambientValue;

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
	vblocktype = blocktype;
	vtime = time;
	vnormal = normal;
	vambientValue = ambientValue;

	directionalLight = normalize((vec4(0,1,0,1) * rot)).xyz;

	vec3 worldPosition = (vec4(position, 1.0) * model).xyz;

	vec3 coordinates = (vec4(1.0, 1.0, 1.0, 1.0) * model).xyz;

	float fac = 2;

	gl_Position = vec4(position.x, position.y, position.z, 1.0) * model * view * projection;

}
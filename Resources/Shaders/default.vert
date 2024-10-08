#version 400 core
layout (location=0) in vec3 position;
layout (location=1) in vec3 normal;
layout (location=2) in vec2 textureCoordinate;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 rotation;

uniform vec3 cameraPosition;
uniform vec3 sunVec;
out vec3 sunNormal;

out vec3 vposition;
out vec3 vnormal;
out vec2 vtextureCoordinate;
out mat4 vmodel;

out mat4 vrotation;

void main()
{

	vposition = position;
	vtextureCoordinate = textureCoordinate;
	vmodel = model;
	sunNormal = sunVec;

	vrotation = rotation;

	gl_Position = vec4(position, 1.0) * model * view * projection;

}
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

	mat4 viewMatrix = transpose(view);

	vec3 cameraRight = vec3(viewMatrix[0][0],viewMatrix[1][0],viewMatrix[2][0]);
	vec3 cameraUp = vec3(viewMatrix[0][1], viewMatrix[1][1], viewMatrix[2][1]);

	// constantly stays the same size regardless of 3d space
	vec4 pos = vec4(0.0, 0.0, 0.0, 1.0) * model * view * projection;
	pos /= pos.w;
	pos.xy += (vec4(position, 1.0) * projection).xy;

	// retains size in 3d space
	pos = vec4(position.x * cameraRight + position.y * cameraUp, 1) * model * view * projection;

	// pos += vec4(position.x * time,position.y * time,0,0) * projection;

	gl_Position = pos;

}
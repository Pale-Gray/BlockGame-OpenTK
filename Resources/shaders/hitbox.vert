#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec3 normal;
layout (location=2) in vec2 texcoord;

out vec3 v_position;
out vec2 v_texcoord;
out vec3 vNormal;
out vec3 viewNormal;
out vec4 viewPosition;

out float v_time;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

float PI = 3.141459265359;

uniform float time;

uniform vec3 cameraPosition;
out vec3 vCameraPosition;
out vec4 worldPosition;
out vec3 cameraEye;
out mat4 modelMatrix;

out mat4 viewMatrix;
out mat4 projectionMatrix;

void main()
{

	v_position = position;
	v_texcoord = texcoord;
	v_time = time;
	vCameraPosition = cameraPosition;

	modelMatrix = model;

	vNormal = normal;
	viewNormal = normalize((vec4(normal, 1.0) * view).xyz);

	// vec4 positionWorld = vec4(position.xyz + 0.5, 1.0) * model * view * projection;
	// worldPosition = (vec4(position.xyz + 0.5, 1.0) * model);

	worldPosition = vec4(position, 1.0) * model;

	vec4 modelViewPosition = vec4(position, 1.0) * model * view;
	viewPosition = modelViewPosition;

	// mat4 viewMatrix = transpose(view);

	viewMatrix = transpose(view);
	projectionMatrix = projection;

	cameraEye = vec3(view[0][2], view[1][2], view[2][2]);

	gl_Position = modelViewPosition * projection;

}
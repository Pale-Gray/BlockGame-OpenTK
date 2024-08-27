#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 texcoord;

out vec3 v_position;
out vec2 v_texcoord;

out float v_time;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

float PI = 3.141459265359;

uniform float time;

void main()
{

	v_position = position;
	v_texcoord = texcoord;

	vec4 positionWorld = vec4(position.xyz, 1.0) * model * view * projection;

	mat4 viewMatrix = transpose(view);

	// vec3 cameraRight = vec3(viewMatrix[0][0], viewMatrix[1][0], viewMatrix[2][0]);
	// vec3 cameraUp = vec3(viewMatrix[0][1], viewMatrix[1][1], viewMatrix[2][1]);
	// vec4 cameraRight = vec4(1,0,0,1) * viewMatrix;
	// vec4 cameraUp = vec4(0, 1, 0, 1) * viewMatrix;

	vec3 cameraRight = vec3(viewMatrix[0][0],viewMatrix[1][0],viewMatrix[2][0]);
	vec3 cameraUp = vec3(viewMatrix[0][1], viewMatrix[1][1], viewMatrix[2][1]);

	vec4 pos = vec4(0.0, 0.0, 0.0, 1.0) * model * view * projection;
	pos /= pos.w;
	pos.xy += (vec4(position, 1.0) * projection).xy;

	pos = vec4(position.x * cameraRight + position.y * cameraUp, 1) * model * view * projection;

	gl_Position = pos;

}
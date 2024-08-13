#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 texcoord;

out vec3 v_position;
out vec2 v_texcoord;

out float v_time;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 sunVec;
out vec3 sunNormal;

float PI = 3.141459265359;

uniform float time;

out vec4 vModelView;
out vec4 vModel;
out vec4 vModelViewProjection;
out vec4 vViewProjection;
out vec4 vView;

out mat4 vModelMatrix;
out mat4 vViewMatrix;
out mat4 vProjectionMatrix;

void main()
{

	sunNormal = sunVec;

	v_position = position;
	v_texcoord = texcoord;
	v_time = time;

	vModel = vec4(position.xyz, 1.0) * model;
	vView = vec4(position.xyz, 1.0) * view;
	vModelView = vModel * view;
	vModelViewProjection = vModelView * projection;
	vViewProjection = vView * projection;

	vModelMatrix = model;
	vViewMatrix = view;
	vProjectionMatrix = projection;

	gl_Position = vec4(position.xyz, 1.0) * model * view * projection;

}
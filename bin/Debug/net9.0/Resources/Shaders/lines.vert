#version 330 core
layout (location=0) in vec3 position;
layout (location=1) in vec3 pointAPosition;
layout (location=2) in vec3 pointBPosition;
layout (location=3) in vec3 color;

out vec3 aPosition;
out vec3 aNormal;
out vec3 aColor;

out float v_time;

uniform mat4 view;
uniform mat4 projection;
uniform vec2 screenDimensions;

uniform vec3 pointAPoint;
uniform vec3 pointBPoint;

uniform float thickness;

float PI = 3.141459265359;

uniform float time;

void main()
{

	aPosition = position;
	aColor = color;

	float val = 0.5;
	int value = int(val);
	
	vec2 resolution = vec2(-screenDimensions.x, screenDimensions.y);

	vec4 viewA = vec4(pointAPosition, 1.0) * view;
	vec4 viewB = vec4(pointBPosition, 1.0) * view;

	vec4 clipA = viewA * projection;
	vec4 clipB = viewB * projection;

	vec2 screenA = resolution * (0.5 * clipA.xy/clipA.w + 0.5);
	vec2 screenB = resolution * (0.5 * clipB.xy/clipB.w + 0.5);

	vec2 xBasis = normalize(screenB - screenA);
	vec2 yBasis = vec2(-xBasis.y, xBasis.x);

	float widthA = thickness/-viewA.z;
	float widthB = thickness/-viewB.z;

	vec2 pointA = screenA + 1 * widthA * (position.x * xBasis + position.y * yBasis);
	vec2 pointB = screenB + 1 * widthB * (position.x * xBasis + position.y * yBasis);
	vec2 point = mix(pointA, pointB, position.z);
	vec4 clip = mix(clipA, clipB, position.z);

	gl_Position = vec4(clip.w * ((2.0 * point)/resolution - 1.0), clip.z, clip.w);

}
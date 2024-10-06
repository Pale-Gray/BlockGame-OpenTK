#version 400 core

layout (location=0) in vec3 position;

uniform int lineSegmentsLength;
uniform vec3[28] lineSegments;
out vec2[28] lineSegmentsPixelPositions;

uniform mat4 projection;
uniform mat4 view;

uniform vec2 screenSize;

void main()
{
	gl_Position = vec4(position, 1.0);

	vec2 resolution = screenSize;

	for (int i = 0; i < lineSegmentsLength; i++)
	{

		vec4 pointAClip = vec4(lineSegments[i], 1.0) * view * projection;
		vec3 pointANdc = pointAClip.xyz / max(pointAClip.w, 0);
		vec2 pointAScreen = ((pointANdc.xy + 1.0) / 2.0) * resolution;
		lineSegmentsPixelPositions[i] = pointAScreen;

	}

}
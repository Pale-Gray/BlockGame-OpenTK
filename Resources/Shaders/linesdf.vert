#version 400 core

layout (location=0) in vec3 position;

uniform int lineSegmentsLength;
uniform vec3[30] lineSegments;

// x and y are screenspace, z is NDC
out vec3[30] lineSegmentsParameters;

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
		vec3 pointANdc = pointAClip.xyz / pointAClip.w;
		vec2 pointAScreen = ((pointANdc.xy + 1.0) / 2.0) * resolution;

		lineSegmentsParameters[i].xy = pointAScreen;
		lineSegmentsParameters[i].z = pointANdc.z;

	}

}
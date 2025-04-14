#version 400 core
out vec4 FragColor;

in vec3 vposition;

uniform mat4 projection;
uniform mat4 view;
uniform vec3 pointA;
uniform vec3 pointB;
uniform vec2 screenSize;

uniform sampler2D sceneDepth;

uniform int lineSegmentsLength;
uniform vec3[30] lineSegments;

in vec3[30] lineSegmentsParameters;

uniform float thickness;
uniform vec3 color;

// from https://iquilezles.org/articles/distfunctions2d/
float sdfLine(vec2 a, vec2 b, vec2 sampl, out float t)
{

	vec2 sampleA = sampl - a, ba = b - a;
	float h = clamp(dot(sampleA,ba)/dot(ba,ba), 0.0, 1.0);
	t = h;
	return length(sampleA - ba*h);

}

float lerp(float a, float b, float t)
{

	return (1 - t) * a + t * b;	

}

void main()
{
	
	float resFactor = 1.0;

	float zNear = 0.1;
	float zFar = 1000.0;

	vec2 pixelCoordinates = gl_FragCoord.xy / resFactor;
	vec2 resolution = screenSize;

	// float testDepth = texture(sceneDepth, pixelCoordinates / resolution).r;

	vec4 colr = vec4(0);
	float lineDepth = 0;
	for (int i = 0; i < lineSegmentsLength; i+=2)
	{

		float t = 0;
		float d = sdfLine(lineSegmentsParameters[i].xy, lineSegmentsParameters[i+1].xy, pixelCoordinates, t) - thickness/2;

		float worldDepth = (texelFetch(sceneDepth, ivec2(mix(lineSegmentsParameters[i].xy, lineSegmentsParameters[i+1].xy, t)), 0).r * 2.0) - 1.0;

		float lineALinearizedDepth = (2.0 * zNear * zFar) / (zFar + zNear - lineSegmentsParameters[i].z * (zFar - zNear));
		float lineBLinearizedDepth = (2.0 * zNear * zFar) / (zFar + zNear - lineSegmentsParameters[i+1].z * (zFar - zNear));
		float lineLinearizedDepth = (2.0 * zNear * zFar) / (zFar + zNear - mix(lineSegmentsParameters[i].z, lineSegmentsParameters[i+1].z, t) * (zFar - zNear));
		float worldLinearizedDepth = (2.0 * zNear * zFar) / (zFar + zNear - worldDepth * (zFar - zNear));

		if (d <= 0 && lineLinearizedDepth < worldLinearizedDepth) 
		{

			FragColor = vec4(vec3(0),1);

		}

		// vec3 col = color;
		// if (pointAClip.w <= 0 || pointBClip.w <= 0) col = vec3(1, 0, 0);

		// float pointALinearDepth = (2.0 * zNear - zFar) / (zFar + zNear - pointANdc.z * (zFar - zNear));
		// float pointBLinearDepth = (2.0 * zNear - zFar) / (zFar + zNear - pointBNdc.z * (zFar - zNear));
		// float testLinearDepth = (2.0 * zNear - zFar) / (zFar + zNear - (testDepth * 2.0 - 1.0) * (zFar - zNear

		// lineDepth = mix((pointANdc.z + 1.0) / 2.0, (pointBNdc.z + 1.0) / 2.0, t);//lerp(pointALinearDepth, pointBLinearDepth, t);
		// vec2 depthCoordinate = mix(((pointANdc.xy + 1.0) / 2.0), ((pointBNdc.xy + 1.0) / 2.0), h);
		// float testDepth = texture(sceneDepth, depthCoordinate).r;
		// float testDepth	= texture(sceneDepth, mix(pointAScreen/resolution, pointBScreen/resolution, t)).r;
		// float testLinearDepth = (2.0 * zNear - zFar) / (zFar + zNear - (testDepth * 2.0 - 1.0) * (zFar - zNear));

	}

}
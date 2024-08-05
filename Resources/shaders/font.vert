#version 400 core
layout (location=0) in vec3 aPosition;
layout (location=1) in vec3 aColor;
layout (location=2) in vec2 aTexCoord;
layout (location=3) in float aIsWiggle;
layout (location=4) in float aIsItalics;

uniform mat4 view;
uniform mat4 projection;

uniform vec3 textPosition;
uniform float time;

out vec2 vTexCoord;
out vec3 vColor;

void main()
{

	vTexCoord = aTexCoord;
	vColor = aColor;

	uint uinte = 0;

	vec3 position = aPosition;

	if (aIsItalics == 1.0)
	{

		if (position.y == textPosition.y)
		{

			position.x -= 8.0;

		}

		if (position.y >= textPosition.y)
		{

			position.x += 8.0;

		}

	}

	if (aIsWiggle == 1.0)
	{

		position.y += 4 * sin(position.x*2 + (2*time));

	}
	
	gl_Position = vec4(position.x, position.y, position.z, 1.0) * view * projection;

}
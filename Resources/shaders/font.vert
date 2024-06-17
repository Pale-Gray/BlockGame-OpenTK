#version 400 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 textureCoordinates;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float time;

out vec2 vtextureCoordinates;

void main()
{

	// vec3 pos = vec3(position.x, position.y + 2*sin((time*5)+(position.x/2)), position.z);
	vec3 pos = position;
	gl_Position = vec4(pos, 1.0) * model * view * projection;

	vtextureCoordinates = textureCoordinates;

}
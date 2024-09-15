#version 400 core
layout (location=0) in vec3 aPosition;
layout (location=1) in vec2 aTexCoord;

uniform mat4 view;
uniform mat4 projection;

uniform vec3 textPosition;
uniform float time;

out vec2 vTexCoord;
out vec3 vColor;

void main()
{

	vTexCoord = aTexCoord;

	gl_Position = vec4(aPosition - vec3(0, 0, 50), 1.0) * view * projection;

}
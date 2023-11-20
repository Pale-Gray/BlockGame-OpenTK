#version 400 core
layout (location=0) in vec3 position;
layout (location=1) in vec2 texCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 v_TexCoords;

void main()
{

	gl_Position = vec4(position.xyz, 1.0) * model * view * projection;

	v_TexCoords = texCoords;

}
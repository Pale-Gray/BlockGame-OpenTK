#version 330 core
out vec4 FragColor;

in vec3 v_position;

in float v_time;

uniform sampler2D tex;

void main()
{

	FragColor = vec4(1.0, 0.0, 0.0, 0.5);

}
#version 330 core
out vec4 FragColor;

in vec3 aPosition;
in vec3 aColor;

in float v_time;

uniform sampler2D tex;

vec3 color = vec3(0,0,0);

void main()
{

	FragColor = vec4(aColor.rgb, 1.0);

}
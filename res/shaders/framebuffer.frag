#version 400 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;
in float v_time;

uniform sampler2D framebufferColorTexture;
uniform sampler2D framebufferDepthStencilTexture;

void main()
{
	float res = 640 * sin(v_time/5);
	vec4 tex = texture(framebufferColorTexture, floor(v_texcoord * res) / res).rgba; // * (intensity + 0.25);

	FragColor = tex.rgba;

}
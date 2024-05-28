#version 400 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;
in float v_time;

uniform sampler2D framebufferColorTexture;
uniform sampler2D framebufferDepthStencilTexture;

void main()
{
	
	vec4 tex = texture(framebufferColorTexture, v_texcoord).rgba; // * (intensity + 0.25);

	FragColor = tex.rgba;

}
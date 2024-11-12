#version 400 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;
in float v_time;

uniform sampler2D framebufferColorTexture;
uniform sampler2D framebufferDepthStencilTexture;


vec3 reinhard_extended(vec3 col, float maxwhite)
{

	vec3 numerator = col * (1.0 + (col / vec3(maxwhite*maxwhite)));
	return numerator / (1.0 + col);

}

float luminance(vec3 color) 
{

	return dot(color, vec3(0.2125, 0.7154, 0.0721));

}

void main()
{
	
	// float gamma = 2.2;
	// float exposure = 0.01;
	// vec3 hdrColor = texture(framebufferColorTexture, v_texcoord).rgb;
	// vec3 mapped = vec3(1.0) - exp(-hdrColor * exposure);
	// vec3 mapped = reinhard_extended(hdrColor, 5.0);
	// mapped = pow(mapped, vec3(1.0 / gamma));
	// vec3 gammaSpace = pow(mapped, vec3(1 / gamma));
	float gamma = 2.2;
	float exposure = 0.0;

	vec3 framebufferColor = texture(framebufferColorTexture, v_texcoord).rgb;
	vec3 map = framebufferColor * exp(exposure);
	map = pow(map, vec3(1/gamma));
	// vec3 mapped = vec3(1.0) - exp(-framebufferColor * exposure);
	// float gamma = 2.2;
	// mapped = pow(mapped, vec3(1.0/gamma));
	// framebufferColor = pow(framebufferColor, vec3(1/gamma));

	FragColor = vec4(map, 1.0);

}
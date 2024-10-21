#version 460 core
#extension GL_ARB_bindless_texture: require
#extension GL_ARB_gpu_shader_int64: require

out vec4 OutColor;

flat in uvec2 vTextureHandle;
in vec2 vTextureCoordinate;

void main()
{

	vec4 glyph = texture(sampler2D(vTextureHandle), vTextureCoordinate);
	OutColor = vec4(1,1,1, glyph.r);//vec4(glyph.rgb, 1);

}
#version 460

out vec4 OutColor;

uniform sampler2DArray fGlyphTexture;

in vec2 vTextureCoordinates;
in flat int vTextureIndex;
in vec4 vGlyphColor;

void main()
{

    float glyph = texture(fGlyphTexture, vec3(vTextureCoordinates, vTextureIndex)).r;

    OutColor = vec4(vGlyphColor.rgb, glyph * vGlyphColor.a);

}
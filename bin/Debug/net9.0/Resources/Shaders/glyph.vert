#version 460

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTextureCoordinates;
layout (location = 2) in int aTextureIndex;
layout (location = 3) in vec4 aGlyphColor;

uniform mat4 uView;
uniform mat4 uProjection;

out vec2 vTextureCoordinates;
out flat int vTextureIndex;
out vec4 vGlyphColor;

void main()
{

    vTextureIndex = aTextureIndex;
    vTextureCoordinates = aTextureCoordinates;
    vGlyphColor = aGlyphColor;

    gl_Position = vec4(aPosition, 0.0, 1.0) * uView * uProjection;

}
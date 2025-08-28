#version 410 core

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTextureCoordinate;

uniform mat4 uProjection;

out vec2 vTextureCoordinate;

void main() {
    vTextureCoordinate = aTextureCoordinate;
    
    gl_Position = vec4(aPosition, -1.0, 1.0) * uProjection;
}
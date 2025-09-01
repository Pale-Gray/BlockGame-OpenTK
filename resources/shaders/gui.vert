#version 410 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTextureCoordinate;
layout (location = 2) in vec3 aColor;

uniform mat4 uProjection;

out vec2 vTextureCoordinate;
out vec3 vColor;

void main() {
    vTextureCoordinate = aTextureCoordinate;
    vColor = aColor;
    gl_Position = vec4(aPosition.xy, aPosition.z - 1.0, 1.0) * uProjection;
}
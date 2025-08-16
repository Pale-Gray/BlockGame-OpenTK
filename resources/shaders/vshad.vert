#version 410 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTextureCoordinate;

uniform mat4 uProjection;
uniform mat4 uView;
uniform vec3 uChunkPosition;

uniform vec3 uPosition;

out vec3 vColor;
out vec2 vTextureCoordinate;
out float vDirectionalLightIntensity;

void main() {
    vColor = aPosition / 32.0;
    vTextureCoordinate = aTextureCoordinate;
    vDirectionalLightIntensity = dot(aNormal, normalize(-vec3(-0.75, -1, 0.5)));
    gl_Position = vec4(aPosition + uPosition, 1.0) * vec4(1, 1, -1, 1) * uView * uProjection;
}
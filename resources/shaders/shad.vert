#version 410 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTextureCoordinate;

uniform mat4 uProjection;
uniform mat4 uView;
uniform vec3 uChunkPosition;

out vec3 vPosition;
out vec3 vNormal;
out vec2 vTextureCoordinate;
out float vDirectionalLightIntensity;

void main() {
    vPosition = (vec4(aPosition + (uChunkPosition * 32.0), 1.0) * vec4(1, 1, -1, 1) * uView * uProjection).xyz;
    vNormal = aNormal;
    vTextureCoordinate = aTextureCoordinate;
    
    gl_Position = vec4(aPosition + (uChunkPosition * 32.0), 1.0) * vec4(1, 1, -1, 1) * uView * uProjection;
}
#version 460

layout (location = 0) in vec3 aPosition;

uniform vec3 uChunkPosition;
uniform mat4 uViewMatrix;
uniform mat4 uProjectionMatrix;

void main()
{

    gl_Position = vec4(aPosition.xyz + (uChunkPosition * 32.0), 1.0) * uViewMatrix * uProjectionMatrix;

}
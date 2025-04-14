#version 460

layout (location = 0) in vec3 aPosition;

uniform mat4 uProjectionMatrix;
uniform mat4 uViewMatrix;
uniform vec3 uCameraPosition;

out vec3 vPosition;

void main()
{

    vPosition = aPosition;

    gl_Position = vec4(aPosition + uCameraPosition, 1.0) * uViewMatrix * uProjectionMatrix;

}
#version 460

layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTextureCoordinate;

out vec4 vColor;
out vec2 vTextureCoordinate;

uniform mat4 view;
uniform mat4 projection;

void main() {

    vColor = aColor;
    vTextureCoordinate = aTextureCoordinate;

    gl_Position = vec4(aPosition, -1.0, 1.0) * view * projection;

}
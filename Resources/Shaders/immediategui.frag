#version 460

out vec4 outColor;

in vec4 vColor;

uniform vec4 uColor;

void main() {

    outColor = vec4(vColor);

}
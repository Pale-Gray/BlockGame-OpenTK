#version 410 core

out vec4 fColor;

uniform sampler2D uTexture;

in vec2 vTextureCoordinate;

void main() {
    vec4 tex = texture(uTexture, vTextureCoordinate);

    fColor = vec4(tex);
}
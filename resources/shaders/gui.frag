#version 410 core

out vec4 fColor;

uniform sampler2D uTexture;

in vec2 vTextureCoordinate;
in vec3 vColor;

void main() {
    vec4 tex = texture(uTexture, vTextureCoordinate);

    if (tex.a == 0.0) discard;
    fColor = vec4(tex * vec4(vColor, 1.0));
}
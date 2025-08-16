#version 410 core

out vec4 fColor;

uniform sampler2D uTexture;

in vec3 vColor;
in vec2 vTextureCoordinate;
in float vDirectionalLightIntensity;

void main() {
    vec3 textureColor = texture(uTexture, vTextureCoordinate).rgb;
    fColor = vec4(textureColor * mix(0.3, 1.0, max(vDirectionalLightIntensity, 0.0)), 1.0);
}
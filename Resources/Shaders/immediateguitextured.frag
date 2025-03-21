#version 460

out vec4 outColor;

in vec4 vColor;
in vec2 vTextureCoordinate;

uniform vec4 uColor;
uniform sampler2D uTexture;

void main() {

    // outColor = vec4(texture(uTexture, vTextureCoordinate).rgb, 1.0);
    vec4 tex = texture(uTexture, vTextureCoordinate).rgba;
    outColor = vec4(tex.rgb, 1.0);

}
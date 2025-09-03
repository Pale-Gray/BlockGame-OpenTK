#version 410 core

out vec4 fColor;
out vec4 fNormal;

uniform sampler2D uTexture;

in vec3 vColor;
in vec3 vNormal;
in vec2 vTextureCoordinate;
in float vDirectionalLightIntensity;

void main() {
    
    vec4 textureColor = texture(uTexture, vTextureCoordinate);
    if (textureColor.a < 1.0)
    {
        if (mod(gl_FragCoord.y, 2.0) > 1.0)
        {
            if (mod(gl_FragCoord.x + 1, 2.0) > 1.0) discard;
        } else
        {
            if (mod(gl_FragCoord.x, 2.0) > 1.0) discard;
        }
    }
    fColor = vec4(textureColor.rgb, 1.0);
    fNormal = vec4(vNormal, 1.0);
}
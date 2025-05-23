#version 460

out vec4 outColor;

in vec3 vNormal;
in vec2 vTextureCoordinate;
in vec4 vLightTopLeft;
in vec4 vLightBottomLeft;
in vec4 vLightBottomRight;
in vec4 vLightTopRight;
flat in uint vTextureIndex;

vec3 sunDir = vec3(0.5, 0.3, 0.4);

uniform sampler2DArray terrainTextures;

uniform float uTime;

void main()
{

    vec3 normal = vNormal;
    if (!gl_FrontFacing) normal *= -1.0;

    float texelScale = 16;

    vec4 topLight = mix(vLightTopLeft, vLightTopRight, floor(vTextureCoordinate.x * texelScale) / texelScale);
    vec4 bottomLight = mix(vLightBottomLeft, vLightBottomRight, floor(vTextureCoordinate.x * texelScale) / texelScale);

    vec4 light = mix(bottomLight, topLight, floor(vTextureCoordinate.y * texelScale) / texelScale);

    float intensity = dot(-normal, normalize(sunDir));

    vec4 albedo = texture(terrainTextures, vec3(vTextureCoordinate, vTextureIndex)).rgba;

    vec3 diffuseColor = vec3(0.5, 0.5, 0.5);

    float easeOutExpo = clamp(1 - pow(2, -10 * uTime), 0.0, 1.0);

    outColor = vec4(albedo.rgb * pow(light.rgb + (light.www), vec3(2.2)), max(mix(-0.5, 1.0, easeOutExpo), 0.0));

}
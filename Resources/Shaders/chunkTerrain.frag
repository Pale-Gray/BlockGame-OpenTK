#version 460

out vec4 outColor;

in vec3 vNormal;
in vec2 vTextureCoordinate;
in vec4 vLightTopLeft;
in vec4 vLightBottomLeft;
in vec4 vLightBottomRight;
in vec4 vLightTopRight;
flat in uint vTextureIndex;

vec3 sunDir = vec3(1, 0.7, 0.4);

uniform sampler2DArray terrainTextures;

void main()
{

    vec4 topLight = mix(vLightTopLeft, vLightTopRight, vTextureCoordinate.x);
    vec4 bottomLight = mix(vLightBottomLeft, vLightBottomRight, vTextureCoordinate.x);

    vec4 light = mix(bottomLight, topLight, vTextureCoordinate.y);

    float intensity = dot(vNormal, sunDir);

    vec4 albedo = texture(terrainTextures, vec3(vTextureCoordinate, vTextureIndex)).rgba;

    vec3 diffuseColor = vec3(0.1, 0.0, 0.8);

    outColor = vec4(albedo.rgb * pow(light.xyz + vec3(0.5), vec3(2.2)), 1.0);

    // outColor = vec4(vTextureCoordinate * intensity, 0, 1);

}
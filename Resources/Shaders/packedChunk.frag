#version 460

out vec4 OutColor;

in vec3 vPosition;
in vec3 vNormal;
in flat int vTextureIndex;

uniform sampler2DArray blockTextureArray;

vec2 euclidean_remainder(vec2 a, vec2 b) {
    
    return a - abs(b)*floor(a/abs(b));
    
}

void main()
{
    
    vec2 textureCoordinates = vec2(0);
    
    if (vNormal == vec3(0, 1, 0)) textureCoordinates = vec2(-1.0, 1.0) * euclidean_remainder(vPosition.xz, vec2(1.0));
    if (vNormal == vec3(0, -1, 0)) textureCoordinates = vec2(-1.0, -1.0) * euclidean_remainder(vPosition.xz, vec2(1.0));
    if (vNormal == vec3(1, 0, 0)) textureCoordinates = vec2(-1.0, 1.0) * euclidean_remainder(vPosition.zy, vec2(1.0));
    if (vNormal == vec3(-1, 0, 0)) textureCoordinates = euclidean_remainder(vPosition.zy, vec2(1.0));
    if (vNormal == vec3(0, 0, 1)) textureCoordinates = euclidean_remainder(vPosition.xy, vec2(1.0));
    if (vNormal == vec3(0, 0, -1)) textureCoordinates = vec2(-1.0, 1.0) * euclidean_remainder(vPosition.xy, vec2(1.0));
    
    vec3 sunlight_position = vec3(0.5, -0.6, 0.3);
            
    vec4 albedo = texture(blockTextureArray, vec3(textureCoordinates, vTextureIndex));
    
    OutColor = vec4(albedo.rgb * (0.2 + max(dot(-vNormal, sunlight_position), 0.0)), 1.0);
    
}
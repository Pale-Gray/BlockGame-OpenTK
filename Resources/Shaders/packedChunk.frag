#version 460

out vec4 OutColor;

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTextureCoordinates;
in vec3 vLightColor;
in flat int vTextureIndex;

uniform sampler2DArray blockTextureArray;

vec2 euclidean_remainder(vec2 a, vec2 b) {
    
    return a - abs(b)*floor(a/abs(b));
    
}

void main()
{
    
    vec3 sunlight_position = vec3(0.5, -0.6, 0.3);
            
    vec4 albedo = texture(blockTextureArray, vec3(vTextureCoordinates, vTextureIndex));
    float alpha = texelFetch(blockTextureArray, ivec3(vTextureCoordinates * 16.0, vTextureIndex), 0).a;

    if (alpha == 0.0) discard;
    OutColor = vec4(vec3(1.0) * (0.01 + pow(vLightColor, vec3(2.2))) * (0.2 + max(dot(-vNormal, sunlight_position), 0.0)), albedo.a);
    
}
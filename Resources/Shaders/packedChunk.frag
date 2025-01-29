#version 460

out vec4 OutColor;

in vec3 vPosition;
in vec3 vNormal;

uniform sampler2DArray blockTextureArray;

void main()
{
    
    vec4 albedo = texture(blockTextureArray, vec3(mod(vPosition.xz, vec2(1.0)), 1.0));
    
    OutColor = vec4(albedo.rgb, 1.0);
    
}
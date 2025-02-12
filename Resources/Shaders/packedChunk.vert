#version 460

layout (location = 0) in uint aPackedVertexData;
layout (location = 1) in uint aPackedColorData;
layout (location = 2) in vec3 aLightColor;

uniform mat4 projection;
uniform mat4 view;

uniform vec3 chunkPosition;

out vec3 vPosition;
out vec3 vNormal;
out vec2 vTextureCoordinates;
out vec3 vLightColor;
out flat int vTextureIndex;

vec3 extractPosition(uint packedData) {
    
    uint xValue = (packedData >> 17) & 63;
    uint yValue = (packedData >> 11) & 63;
    uint zValue = (packedData >> 5) & 63;
    
    return vec3(xValue, yValue, zValue);
    
}

vec3 normalIndex[] = {
    vec3(0, 1, 0),
    vec3(0, -1, 0),
    vec3(1, 0, 0),
    vec3(-1, 0, 0),
    vec3(0, 0, -1),
    vec3(0, 0, 1)
};

vec2 textureCoordinates[] = {
    vec2(0, 1),
    vec2(0, 0),
    vec2(1, 0),
    vec2(1, 1)
};

vec3 extractNormal(uint packedData) {
    
    return normalIndex[int((packedData & 28) >> 2)]; // 28 equivalent to 0b11100
    
}

void main() {
    
    vPosition = extractPosition(aPackedVertexData);
    vNormal = extractNormal(aPackedVertexData);
    vTextureCoordinates = textureCoordinates[int(aPackedVertexData & 3)];
    vLightColor = aLightColor;
    vTextureIndex = int(aPackedColorData & 65535);
    gl_Position = vec4(vPosition + (chunkPosition * 32), 1) * view * projection;
    
}
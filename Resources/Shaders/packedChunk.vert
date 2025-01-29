#version 460

layout (location = 0) in uint aPackedVertexData;
layout (location = 1) in uint aVertexColorData;

uniform mat4 projection;
uniform mat4 view;

uniform vec3 chunkPosition;

out vec3 vPosition;
out vec3 vNormal;

vec3 extractPosition(uint packedData) {
    
    uint xValue = (packedData >> 15) & 63;
    uint yValue = (packedData >> 9) & 63;
    uint zValue = (packedData >> 3) & 63;
    
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

vec3 extractNormal(uint packedData) {
    
    return normalIndex[int(packedData & 7)];
    
}

void main() {
    
    vPosition = extractPosition(aPackedVertexData);
    vNormal = extractNormal(aPackedVertexData);
    gl_Position = vec4(vPosition + (chunkPosition * 32), 1) * view * projection;
    
}
#version 460

uniform mat4 projection;
uniform mat4 view;
uniform mat4 conversion;
uniform vec3 chunkPosition;

out vec3 vNormal;
out vec2 vTextureCoordinate;
out flat uint vTextureIndex;
out vec4 vLightTopLeft;
out vec4 vLightBottomLeft;
out vec4 vLightBottomRight;
out vec4 vLightTopRight;

struct Rectangle
{
    vec4 lightTopLeft;
    vec4 lightBottomLeft;
    vec4 lightBottomRight;
    vec4 lightTopRight;
    vec3 position;
    vec3 tangent;
    vec3 bitangent;
    vec2 uvSize;
    vec2 uvOffset;
    vec2 size;
    uint textureIndex;
};

layout(std430, binding = 0) buffer Rectangles
{
    Rectangle solids[];
};

vec2 vertexFactors[] = vec2[]
(

    vec2(1, 1),
    vec2(1, 0),
    vec2(0, 0),
    vec2(0, 1)

);

vec2 textureMapFactors[] = vec2[]
(
    vec2(0, 1),
    vec2(0, 0),
    vec2(1, 0),
    vec2(1, 1)
);

void main()
{

    int index = gl_VertexID / 4;

    vec3 position = solids[index].position;
    vec3 tangent = solids[index].tangent;
    vec3 bitangent = solids[index].bitangent;
    vec2 uvOffset = solids[index].uvOffset;
    vec2 uvSize = solids[index].uvSize;
    vec2 size = solids[index].size;
    uint textureIndex = solids[index].textureIndex;

    vec2 factor = vertexFactors[gl_VertexID % 4];
    vec2 textureFactor = textureMapFactors[gl_VertexID % 4];

    vNormal = cross(tangent, bitangent);
    vTextureCoordinate = uvOffset + (uvSize * textureFactor);
    vTextureIndex = textureIndex;
    vLightTopLeft = solids[index].lightTopLeft;
    vLightBottomLeft = solids[index].lightBottomLeft;
    vLightBottomRight = solids[index].lightBottomRight;
    vLightTopRight = solids[index].lightTopRight;
    
    vec3 pos = position + ((tangent * size.y) * factor.y) + ((bitangent * size.x) * factor.x);

    gl_Position = vec4(pos + (chunkPosition * 32.0), 1) * view * projection;

}
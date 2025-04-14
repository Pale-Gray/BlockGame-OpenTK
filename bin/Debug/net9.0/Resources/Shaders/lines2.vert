#version 460

layout (location=0) in vec3 aPosition;
layout (location=1) in vec3 aPositionA;
layout (location=2) in vec3 aPositionB;

uniform mat4 view;
uniform mat4 projection;
uniform mat4 model;

uniform vec2 resolution;

void main()
{
    
    float width = 25.0;
    
    vec4 clip0 = vec4(aPositionA, 1.0) * model * view * projection;
    vec4 clip1 = vec4(aPositionB, 1.0) * model * view * projection;
    
    vec2 screen0 = (0.5 * clip0.xy/clip0.w + 0.5) * resolution;
    vec2 screen1 = (0.5 * clip1.xy/clip1.w + 0.5) * resolution;
    
    vec2 xBasis = normalize(screen1 - screen0);
    vec2 yBasis = vec2(-xBasis.y, xBasis.x);
    vec2 pt0 = (aPosition.x * xBasis + aPosition.y * yBasis) * width + screen0;
    vec2 pt1 = (aPosition.x * xBasis + aPosition.y * yBasis) * width + screen1;
    vec2 pt = mix(pt0, pt1, aPosition.z);
    
    vec4 clip = mix(clip0, clip1, aPosition.z);

    gl_Position = vec4(clip.w * ((2.0 * pt) / resolution - 1.0), clip.z, clip.w);
    
}
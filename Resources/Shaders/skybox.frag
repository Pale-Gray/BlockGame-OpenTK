#version 460

out vec4 outColor;

uniform int uTickTime;
uniform sampler2D uSkyColor;

in vec3 vPosition;

float chebDist(vec3 a, vec3 b)
{

    return max(abs(b.x-a.x), max(abs(b.y-a.y), abs(b.z-a.z)));

}

vec3 calculateSunVector(int tickTime)
{

    return vec3(cos(radians((tickTime / 36000.0) * 360.0)), sin(radians((tickTime / 36000.0) * 360.0)), 0);

}

void main()
{

    int tickTime = uTickTime;

    vec3 rayDirection = normalize(vPosition);

    float intensity = (dot(rayDirection, vec3(0.0, 1.0, 0.0)) + 1.0) / 2.0;

    vec4 color = texture(uSkyColor, vec2(mod((tickTime / 36000.0), 1.0), intensity));
    float val = 1.0;
    if (chebDist(rayDirection, calculateSunVector(tickTime)) < 0.1) val = 0.0;
    color *= val;

    outColor = vec4(color.rgb, 1);

}
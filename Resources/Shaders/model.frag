#version 460 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;

in vec3 sunNormal;

in float v_time;

in vec4 vModelView;
in vec4 vModel;
in vec4 vModelViewProjection;
in vec4 vView;
in vec4 vViewProjection;

in mat4 vModelMatrix;
in mat4 vViewMatrix;
in mat4 vProjectionMatrix;

uniform sampler2D tex;

void main()
{

	vec4 tex = (texture(tex, v_texcoord).rgba); // * (intensity + 0.25);

	// FragColor = vec4(mix(color1*1.2, color1*1.4, v_position.y*1.2),1);//tex.rgba;

	vec3 color1 = vec3(155.0/255, 197.0/255, 237.0/255);
	vec3 color2 = vec3(13.0/255, 15.0/255, 48.0/255);

	vec3 daytopColor = vec3(0.251,0.227,0.945);
	vec3 daybottomColor = vec3(0.522,0.667,0.933);

	vec3 sunsetTopColor = vec3(0.5,0,0);
	vec3 sunsetBottomColor = vec3(1,0,0);

	float factor = dot(vec3(0,1,0), normalize(v_position));

	vec3 dayColor = mix(daytopColor, daybottomColor, 1 - clamp(factor, 0, 1));

	vec3 sunsetColor = mix(sunsetTopColor, sunsetBottomColor, 1 - clamp(factor, 0, 1));

	vec3 nightTopColor = vec3(0,0,0);
	vec3 nightBottomColor = vec3(0,0,0);

	vec3 nightColor = mix(nightTopColor, nightBottomColor, 1 - clamp(factor, 0, 1));

	float fac = clamp(dot(vec3(0,-1,0), normalize(sunNormal.xyz)), 0, 1);

	if (tex.a == 0) discard;

	vec3 finalColor = mix(dayColor, mix(sunsetColor, nightColor, (1-fac)/1), (1-fac)/1);

	// clamp(fac, 0, 1))
	FragColor = vec4(pow(finalColor, vec3(2.2)), tex.a);
	// FragColor = vec4(0,0,0,1);
}
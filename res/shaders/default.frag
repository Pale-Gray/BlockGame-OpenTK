#version 400 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;
in vec3 v_normal;
in float v_blocktype;
in mat4 v_model;

in vec3 camera_position;
in vec3 c_position;

in vec4 vpos;

in float v_time;

uniform sampler2D tex;
uniform sampler2D emission;
uniform samplerCube cubemap;

in vec3 _position;
in vec3 _normal;

in float intensity;

float dist(vec3 p1, vec3 p2) {



	return sqrt(pow(p2.x-p1.x,2)+pow(p2.y-p1.y,2)+pow(p2.z-p1.z,2));

}

vec3 reduceColor(vec3 color, int bit_depth) {

	float coldiv = pow(2, bit_depth);


    return vec3(floor(color.r*coldiv)/coldiv, floor(color.g*coldiv)/coldiv, floor(color.b*coldiv)/coldiv);

}

void main()
{
	
	float lighting = 1;

	vec4 tex = (texture(tex, v_texcoord.xy).rgba); // * (intensity + 0.25);

	float ambient = 0.2;

	vec3 lightnormal = vec3(0.2,-0.6,0.8);

	float multiplier = 1.5;
	float brightness = max(0.0, dot(normalize(-lightnormal), v_normal));

	vec4 color = vec4(tex.rgb * (ambient + brightness), tex.a) + vec4(texture(emission, v_texcoord.xy).rgb, 0.0);
	float s = abs(sin(v_time/5))*8;
	vec3 col = reduceColor(color.rgb, int(s));

	

	vec3 I = normalize(_position - camera_position);
	vec3 R = reflect(I, normalize(_normal));

	vec4 outc = vec4(tex.rgb * (ambient + brightness), tex.a);
	// FragColor = textureCube(cubemap, R);
	FragColor = vec4(tex.rgb * (ambient + brightness), tex.a);
	//FragColor = vec4(v_normal, 1);

}
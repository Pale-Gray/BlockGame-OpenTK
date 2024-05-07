#version 400 core
layout (location=0) in float blocktype;
layout (location=1) in vec3 position;
layout (location=2) in vec3 normal;
layout (location=3) in vec2 texcoord;

out vec3 v_position;
out vec3 v_wposition;
out vec3 v_normal;
out vec2 v_texcoord;
out float v_blocktype;
out float v_time;
out mat4 v_model;

out vec3 c_position;

out vec3 camera_position;

out vec4 vpos;

out vec3 _normal;
out vec3 _position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 cpos;
uniform vec3 campos;

float PI = 3.141459265359;

uniform float time;

out float intensity;

float dist(vec3 p1, vec3 p2) {

	return sqrt(pow(p2.x-p1.x,2)+pow(p2.y-p1.y,2)+pow(p2.z-p1.z,2));

}

void main()
{

	v_position = position;
	v_normal = normal;
	v_texcoord = texcoord;
	v_blocktype = blocktype;
	v_time = time;
	v_model = model;  

	c_position = cpos;
	camera_position = campos;

	_normal = mat3(transpose(inverse(model))) * normal;
	_position = vec3(model * vec4(position.xyz, 1.0));

	float s = 0.1;
	v_wposition = vec3(vec4(floor(position.xyz*s)/s, 1.0) * model);

	gl_Position = vec4(position.xyz, 1.0) * model * view * projection;
	// gl_Position.xyz = floor(gl_Position.xyz * 16)/16;

	vec3 lightpos = vec3(16+cos(time),5,16+sin(time));

	float radius = 12;

	intensity = max(dot(normalize(lightpos-position), v_normal), 0);
}
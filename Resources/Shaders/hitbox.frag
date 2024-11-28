#version 330 core
out vec4 FragColor;

in vec3 v_position;
in vec2 v_texcoord;

in float v_time;

uniform sampler2D tex;

uniform float time;

in vec3 vCameraPosition;
in vec4 worldPosition;
in vec3 cameraEye;

in mat4 viewMatrix;
in mat4 projectionMatrix;
in mat4 modelMatrix;

in vec4 viewPosition;

in vec3 vNormal;
in vec3 viewNormal;

in float distanceToCenter;

vec2 rotate(in vec2 coordinates, in float rotationDegrees)
{

	float degreesRadians = radians(rotationDegrees);

	mat2x2 rotationMatrix = mat2x2(cos(degreesRadians), sin(degreesRadians), -sin(degreesRadians), cos(degreesRadians));

	return rotationMatrix * coordinates;

}

void main()
{

	vec3 tangent = vec3(vNormal.y, 0, 0);
	vec3 bitangent = vec3(0, 0, vNormal.y);
	mat3 TBN = mat3(tangent, bitangent, vNormal);

	vec3 cameraViewDirection = vCameraPosition - worldPosition.xyz;
	vec3 cameraTangentSpaceViewDirection = normalize(TBN * cameraViewDirection);

	vec4 image;

	vec2 textureCoordinates = worldPosition.xz;

	// vec4 color = texture(tex, (rotate(textureCoordinates, v_time*45)/5) - (cameraTangentSpaceViewDirection.xy / cameraTangentSpaceViewDirection.z) * 0).rgba;
	// image += vec4(color.rgb, color.a);

	for (float i = 0; i < 5; i++)
	{

		vec4 color = texture(tex, rotate(((textureCoordinates*2.0)) - ((cameraTangentSpaceViewDirection.xy / (cameraTangentSpaceViewDirection.z))) * (0.01 + (i/8.0)), (v_time/45)*180));
		image += vec4(color.rgb, color.a);

	}
	// vec4 image = texture(tex, v_texcoord - (cameraTangentSpaceViewDirection.xy / cameraTangentSpaceViewDirection.z)).rgba;
	
	// FragColor = vec4(image.rgb, image.a);

	vec4 img = texture(tex, v_texcoord);
	if (img.a == 0.0) discard;

	// FragColor = vec4(img.rgb * (vec3(0)), img.a - (abs(sin(time*2.0)) / 8.0));

	FragColor = vec4(img.rgb * 0.0, 0.9);

}
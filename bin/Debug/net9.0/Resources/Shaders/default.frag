#version 400 core
out vec4 FragColor;

in vec3 vposition;
in vec3 vnormal;
in vec2 vtextureCoordinate;

uniform sampler2D sunTexture;

float dist(vec3 p1, vec3 p2) {



	return sqrt(pow(p2.x-p1.x,2)+pow(p2.y-p1.y,2)+pow(p2.z-p1.z,2));

}

vec3 reduceColor(vec3 color, int bit_depth) {

	float coldiv = pow(2, bit_depth);


    return vec3(floor(color.r*coldiv)/coldiv, floor(color.g*coldiv)/coldiv, floor(color.b*coldiv)/coldiv);

}

void main()
{
	
	// FragColor = vec4(2*vposition.xyz,1);
	FragColor = texture(sunTexture, vtextureCoordinate);
	// FragColor = vec4(0,0,0,1);

}
#version 400 core
out vec4 FragColor;

in vec2 vTextureCoordinates;
in vec2 vGuiSpacePosition;

uniform sampler2D guiTexture;

uniform vec2 guiPosition;
uniform vec2 guiSize;
uniform flat int guiBorderRadius;
uniform flat int guiBorderRadiusType;
uniform vec3 guiColor;

float manhattan_distance(vec2 a, vec2 b) {

	return abs(a.x-b.x) + abs(a.y-b.y);

}

void main()
{

	float minRadius = min(guiSize.x, guiSize.y) / 2.0;
	float borderRadius = min(minRadius, guiBorderRadius);

	if (borderRadius > 0) {

		// if the sampled pixel x is less than where the area of which the border radius should start
		if (vGuiSpacePosition.x < guiPosition.x + borderRadius) {

			// top portion
			if (vGuiSpacePosition.y < guiPosition.y + borderRadius) {

				if (guiBorderRadiusType != 0) {

					if (manhattan_distance(vGuiSpacePosition, guiPosition + borderRadius) > borderRadius) discard;

				} else {

					if (distance(vGuiSpacePosition, guiPosition + borderRadius) > borderRadius) discard;

				}

			}

			// bottom portion
			if (vGuiSpacePosition.y > (guiPosition.y + guiSize.y - borderRadius)) {

				if (guiBorderRadiusType != 0) {

					if (manhattan_distance(vGuiSpacePosition, guiPosition + vec2(borderRadius, guiSize.y - borderRadius)) > borderRadius) discard;

				} else {

					if (distance(vGuiSpacePosition, guiPosition + vec2(borderRadius, guiSize.y - borderRadius)) > borderRadius) discard;

				}

			}

		}

		if (vGuiSpacePosition.x > guiPosition.x + guiSize.x - borderRadius) {

			if (vGuiSpacePosition.y < guiPosition.y + borderRadius) {

				if (guiBorderRadiusType != 0) {

					if (manhattan_distance(vGuiSpacePosition, guiPosition + vec2(guiSize.x - borderRadius, borderRadius)) > borderRadius) discard;

				} else {

					if (distance(vGuiSpacePosition, guiPosition + vec2(guiSize.x - borderRadius, borderRadius)) > borderRadius) discard;

				}

			}

			if (vGuiSpacePosition.y > guiPosition.y + guiSize.y - borderRadius) {

				if (guiBorderRadiusType != 0) {

					if (manhattan_distance(vGuiSpacePosition, guiPosition + (guiSize - borderRadius)) > borderRadius) discard;

				} else {

					if (distance(vGuiSpacePosition, guiPosition + (guiSize - borderRadius)) > borderRadius) discard;

				}

			}

		}

	}

	vec4 guiTex = texture(guiTexture, vTextureCoordinates);
	FragColor = vec4(guiTex.rgb * guiColor, guiTex.a);

}
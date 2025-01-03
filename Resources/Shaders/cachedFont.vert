#version 460 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 textureCoordinate;
layout (location = 2) in vec4 glyphColor;
layout (location = 3) in float glyphTextureIndex;
layout (location = 4) in vec4 glyphWiggleWaggleData;
layout (location = 5) in uint glyphFormatFlags;
layout (location = 6) in float glyphIndex;
layout (location = 7) in float glyphWiggleWaggleSmoothingFactor;
layout (location = 8) in float glyphItalicPortion;

uniform mat4 view;
uniform mat4 projection;

out vec2 vTextureCoordinate;
out vec4 vGlyphColor;
out float vGlyphTextureIndex;

uniform float time;
uniform float textSize;

void main() 
{

	vec2 wiggleWaggle = vec2(0,0);
	if ((glyphFormatFlags >> 1 & 1) == 1) 
	{

		wiggleWaggle += vec2(glyphWiggleWaggleData.x * sin((time * glyphWiggleWaggleData.y) + (glyphIndex * glyphWiggleWaggleSmoothingFactor)), glyphWiggleWaggleData.z * cos((time * glyphWiggleWaggleData.w) + (glyphIndex * glyphWiggleWaggleSmoothingFactor)));	

	}
	if ((glyphFormatFlags & 1) == 1) {

		if (glyphItalicPortion == 1) {

			wiggleWaggle.x += 0.2 * textSize;
			
		}

	}

	gl_Position = vec4(position.xy + wiggleWaggle, -1.0 + -((10000.0 - position.z) / 10000.0), 1.0) * view * projection;

	vTextureCoordinate = textureCoordinate;
	vGlyphColor = glyphColor;
	vGlyphTextureIndex = glyphTextureIndex;

}


#version 460 core

out vec4 OutColor;

in vec2 vTextureCoordinate;
in vec3 vGlyphColor;
in float vGlyphTextureIndex;

uniform sampler2DArray glyphTextureArray;
uniform float time;
uniform vec3 color;

// https://www.easyrgb.com/en/math.php
vec3 toRgb(float hue, float saturation, float value)
{
	
	if (saturation == 0) 
	{

		return vec3(value);

	} else
	{

		float varH = hue * 6.0;
		if (varH == 6.0) varH = 0.0;
		float varI = floor(varH);
		float var1 = value * (1 - saturation);
		float var2 = value * (1 - saturation * (varH - varI));
		float var3 = value * (1 - saturation * (1 - (varH - varI)));
		float r = 0;
		float g = 0;
		float b = 0;

		if		(varI == 0) { r = value; g = var3; b = var1; }
		else if (varI == 1) { r = var2; g = value; b = var1; }
		else if (varI == 2) { r = var1; g = value; b = var3; }
		else if (varI == 3) { r = var1; g = var2; b = value; }
		else if (varI == 4) { r = var3; g = var1; b = value; }
		else				{ r = value; g = var1; b = var2; }

		return vec3(r,g,b);

	}

}

void main()
{

	float alpha = texture(glyphTextureArray, vec3(vTextureCoordinate, vGlyphTextureIndex)).r;
	if (alpha == 0.0) discard;
	OutColor = vec4(vGlyphColor, alpha);

}
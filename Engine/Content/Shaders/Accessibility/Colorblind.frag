#version 440 core

in vec2 fSource;
in vec4 fColor;

out vec4 fragColor;

uniform sampler2D image;
uniform int filterType;
uniform int gammaToLinear[256];
uniform int linearToGamma[256];

int GammaCorrect(int value)
{
	if (value < 0)
	{
		return 0;
	}
	
	if (value > 255)
	{
		return 255;
	}

	value = linearToGamma[value];
	
	return value >= 0 ? value : 256 + value;
}

vec4 RedGreen(vec4 color, int k1, int k2, int k3)
{
	int r = gammaToLinear[int(color.r * 255)];
	int g = gammaToLinear[int(color.g * 255)];
	int b = gammaToLinear[int(color.b * 255)];

	int rBlind = int(k1 * r + k2 * g) >> 22;
	int bBlind = int(k3 * r - k3 * g + 32768 * b) >> 22;

	rBlind = clamp(rBlind, 0, 255);
	bBlind = clamp(bBlind, 0, 255);

	int red = linearToGamma[rBlind];
	int blue = linearToGamma[bBlind];

	red = red >= 0 ? red : 256 + red;
	blue = blue >= 0 ? blue : 256 + blue;

	color.r = red / 255.0;
	color.g = red / 255.0;
	color.b = blue / 255.0;

	return color;
}

vec4 Tritanopia(vec4 color)
{
	const float e0 = 0.14597772;
	const float e1 = 0.12188395;
	const float e2 = 0.08413913;
	const float inflection = e1 / e0;
	const float a1 = -e2 * 0.007009;
	const float b1 = e2 * 0.0914;
	const float c1 = e0 * 0.007009 - e1 * 0.0914;
	const float a2 = e1 * 0.3636 - e2 * 0.2237;
	const float b2 = e2 * 0.1284 - e0 * 0.3636;
	const float c2 = e0 * 0.2237 - e1 * 0.1284;

	int r = gammaToLinear[int(color.r * 255)];
	int g = gammaToLinear[int(color.g * 255)];
	int b = gammaToLinear[int(color.b * 255)];

	float l = (r * 0.05059983 + g * 0.08585369 + b * 0.00952420) / 32767;
	float m = (r * 0.01893033 + g * 0.08925308 + b * 0.01370054) / 32767;
	float temp = m / l;
	float s = temp < inflection
		? -(a1 * l + b1 * m) / c1
		: -(a2 * l + b2 * m) / c2;

	int iRed = int(255 * (l * 30.830854 - m * 29.832659 + s * 1.610474));
	int iGreen = int(255 * (-l * 6.481468 + m * 17.715578 - s * 2.532642));
	int iBlue = int(255 * (-l * 0.375690 - m * 1.199062 + s * 14.273846));

	iRed = GammaCorrect(iRed);
	iGreen = GammaCorrect(iGreen);
	iBlue = GammaCorrect(iBlue);

	color.r = iRed / 255.0;
	color.g = iGreen / 255.0;
	color.b = iBlue / 255.0;

	return color;
}

vec4 Grayscale(vec4 color)
{
	int r = gammaToLinear[int(color.r * 255)];
	int g = gammaToLinear[int(color.g * 255)];
	int b = gammaToLinear[int(color.b * 255)];

	float luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
	float result = GammaCorrect(int(luminance) >> 8) / 255.0;

	color.r = result;
	color.g = result;
	color.b = result;

	return color;
}

// See https://github.com/nvkelso/color-oracle-java/blob/master/src/ika/colororacle/Simulator.java.
void main()
{
	vec4 color = texture(image, fSource);

	switch (filterType)
	{
		case 0: color = RedGreen(color, 9591, 23173, -730); break;
		case 1: color = RedGreen(color, 3683, 29084, 131); break;
		case 2: color = Tritanopia(color); break;
		case 3: color = Grayscale(color); break;
	}

	fragColor = fColor * color;
}

#version 440 core

const int MaxMaterials = 6;

in vec3 fPosition;
in vec2 fSource;
in flat float fSourceIndex;
in vec3 fNormal;
in vec4 fShadowMapCoords;

out vec4 fragColor;

uniform vec3 eye;
uniform vec3 lightColor;
uniform vec3 lightDirection;
uniform float ambientIntensity;

// Right now, only specular values are stored in each material. 
uniform float materials[MaxMaterials];
uniform sampler2D textureSamplers[MaxMaterials];
uniform sampler2D shadowSampler;

void main()
{
	vec4 color = texture(textureSamplers[int(fSourceIndex)], fSource);

	float shadowValue = texture(shadowSampler, fShadowMapCoords.xy).r;
	float d = dot(-lightDirection, fNormal);
	float bias = 0.001;
	float lightIntensity;

	if (fShadowMapCoords.z - bias > shadowValue)
	{
		lightIntensity = ambientIntensity;
	}
	else
	{
		// Compute diffuse.
		float diffuse = clamp(d, 0, 1);

		// Compute specular.	
		vec3 viewDirection = normalize(fPosition - eye);
		vec3 reflected = reflect(lightDirection, fNormal);
		float s1 = materials[int(fSourceIndex)] * 25;
		float specular = 0;

		if (s1 > 0)
		{
			float s2 = pow(max(dot(-viewDirection, reflected), 0.0), s1);

			specular = s1 * s2;
		}

		// Compute final intensity.
		lightIntensity = clamp(ambientIntensity + diffuse + specular, 0, 1);
	}

	fragColor = color * vec4(lightColor * lightIntensity, 1);
}

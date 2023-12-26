#version 460

uniform sampler2D textureAmbient;
uniform sampler2D textureSpecDiff;
uniform sampler2D textureShadows;

out vec4 FragColor;

float sigmaS = 3;
float sigmaL = 3;

#define EPS 1e-5

float lum(in vec4 color) { return length(color.xyz); }

vec4 bilateralFilter()
{
    float sigS = max(sigmaS, EPS);
    float sigL = max(sigmaL, EPS);

    float facS = -1. / (2. * sigS * sigS);
    float facL = -1. / (2. * sigL * sigL);

    float sumW         = 0.;
    vec4 sumC          = vec4(0.);
    float halfSize     = sigS * 2;
    ivec2 textureSize2 = textureSize(textureShadows, 0);

    vec2 texCoord = gl_FragCoord.xy / textureSize(textureSpecDiff, 0);
    float l       = lum(texture(textureShadows, texCoord));

    for (float i = -halfSize; i <= halfSize; i++) {
        for (float j = -halfSize; j <= halfSize; j++) {
            vec2 pos         = vec2(i, j);
            vec4 offsetColor = texture(textureShadows, texCoord + pos / textureSize2);

            float distS = length(pos);
            float distL = lum(offsetColor) - l;

            float wS = exp(facS * float(distS * distS));
            float wL = exp(facL * float(distL * distL));
            float w  = wS * wL;

            sumW += w;
            sumC += offsetColor * w;
        }
    }

    return sumC / sumW;
}


void blend()
{
    vec2 texCoord = gl_FragCoord.xy / textureSize(textureSpecDiff,0);

    vec4 shadow = bilateralFilter();
    shadow = vec4(shadow.xxx, 2.);
    shadow = 1 - shadow;
    // vec4 shadow = vec4(1.);

    vec4 ambient = texture(textureAmbient, texCoord);
    vec4 specularDiffuse = texture(textureSpecDiff, texCoord);
    FragColor = ambient + specularDiffuse * shadow;
}


void main()
{
    blend();
    vec2 texCoord = gl_FragCoord.xy / textureSize(textureSpecDiff,0);
    // FragColor = texture(textureSpecDiff, texCoord);

    // FragColor = texture(textureAmbient, texCoord);
    // vec4 filtered = texture(textureShadows, texCoord);
    // vec4 filtered = bilateralFilter();
    // vec4 filtered = customFilter();
    // FragColor = vec4(filtered.x, filtered.x, filtered.x, 1.0);
    // FragColor = vec4(ts.x/2., 0, 0.0, 1.0);
}

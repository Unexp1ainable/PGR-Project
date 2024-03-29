#version 460

uniform sampler2D textureAmbient;
uniform sampler2D textureReflection;
uniform sampler2D textureRefraction;
uniform sampler2D textureShadows;
uniform sampler2D texturePrimary;

out vec4 FragColor;

uniform uint accumCounter = 1;
float sigmaS              = 3;
float sigmaL              = 3;

#define EPS 1e-5

// https://github.com/tranvansang/bilateral-filter
vec4 bilateralFilter()
{
    float sigS = max(sigmaS, EPS);
    float sigL = max(sigmaL, EPS);

    float facS = -1. / (2. * sigS * sigS);
    float facL = -1. / (2. * sigL * sigL);

    vec4 sumW         = vec4(0.);
    vec4 sumC          = vec4(0.);
    float halfSize     = sigS * 2;
    ivec2 textureSize2 = textureSize(textureShadows, 0);

    vec2 texCoord = gl_FragCoord.xy / textureSize(textureAmbient, 0);
    vec4 l        = texture(textureShadows, texCoord);
    for (float i = -halfSize; i <= halfSize; i++) {
        for (float j = -halfSize; j <= halfSize; j++) {
            vec2 pos = vec2(i, j);
            vec4 offsetColor = texture(textureShadows, texCoord + pos / textureSize2);
            for (int c = 0; c < 3; c++) {

                float chcol = offsetColor[c];
                float chl   = l[c];

                float distS = length(pos);
                float distL = chcol - chl;

                float wS = exp(facS * float(distS * distS));
                float wL = exp(facL * float(distL * distL));
                float w  = wS * wL;

                sumW[c] += w;
                sumC[c] += chcol * w;
            }
        }
    }

    return sumC / sumW;
}

// filter shadows and combine with other textures
void main()
{
    vec2 texCoord = gl_FragCoord.xy / textureSize(textureAmbient, 0);

    vec4 shadow = bilateralFilter();
    shadow      = vec4(1.) - shadow;

    vec4 ambient    = texture(textureAmbient, texCoord);
    vec4 reflection = texture(textureReflection, texCoord);
    vec4 refraction = texture(textureRefraction, texCoord);
    vec4 primary    = texture(texturePrimary, texCoord);

    FragColor = ambient + refraction * shadow.z + reflection * shadow.y + primary * shadow.x;
}

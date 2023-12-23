#version 460
layout(location = 0) out float fShadow;
layout(location = 1) out vec4 fSpecDiff;
layout(location = 2) out vec4 fAmbient;

in vec4 gl_FragCoord;


#define ORIGIN       vec3(0, 0, 0)
#define EPSILON      0.00001
#define GOLDEN_RATIO 1.618033988749895
#define PI           3.14159265359

#define TYPE_SPHERE          0
#define TYPE_CYLINDER        1
#define TYPE_CAPPED_CYLINDER 2
#define TYPE_PLANE           3
#define TYPE_DISC            4
#define TYPE_POINT_LIGHT     5


#define FIGURE_PAWN   0
#define FIGURE_KING   1
#define FIGURE_QUEEN  2
#define FIGURE_BISHOP 3
#define FIGURE_KNIGHT 4
#define FIGURE_ROOK   5

#define COLOR_WHITE 0
#define COLOR_BLACK 1

struct Material {
    vec3 color; // diffuse color
    float n; // refraction index
    float roughness; // Cook-Torrance roughness
    float transparency; // fraction of light that passes through the object
    float density; // Cook-Torrance color density i.e. fraction of diffuse reflection
};

struct Hit {
    float t;
    vec3 normal; // normal of the surface at hit point
    bool inside;
};

#define NO_HIT Hit(-1, vec3(0), false)


struct HitInfo {
    Hit hit;
    vec3 ed; // eye direction
    Material material;
    bool isLight;
};

uniform int screenWidth       = 1024;
uniform int screenHeight      = 768;
uniform mat4 cameraMatrix     = mat4(1.0);
uniform int reflectionBounces = 3;
uniform int shadowRays        = 32;
uniform vec3 lightPosition    = vec3(0, 4, 0);
uniform float n               = 1.2;
uniform uint time             = 42;

uniform float roughness    = 0.68;
uniform float transparency = 0.5;
uniform float density      = 0.8;

Material materialRed   = Material(vec3(1, 0, 0), n, 0.1, 1., 0.);
Material materialGreen = Material(vec3(0, 1, 0), 1.4, roughness, transparency, density);
Material materialBlue  = Material(vec3(0, 0, 1), 2.5, roughness, transparency, density);
Material materialGray  = Material(vec3(0.2, 0.2, 0.2), 1., roughness, 0., 1.);
Material materialWhite = Material(vec3(1, 1, 1), 1., roughness, 0., 1.);

struct LightItem {
    vec3 position;
    float radius;
    Material material;
};

LightItem light = LightItem(vec3(0, 4, 0), 0.5, Material(vec3(1, 1, 1), 1., roughness, 0., 1.));

#define LIGHT_COUNT 1
LightItem[LIGHT_COUNT] lights = {
    light,
    // light2,
};

Hit intersectSphere(vec3 ro, vec3 rd, vec3 center, float radius)
{
    vec3 sphereToRay = ro - center;

    float a = dot(rd, rd);
    float b = 2 * dot(rd, sphereToRay);
    float c = dot(center, center) + dot(ro, ro) - 2.0 * dot(center, ro) - radius * radius;
    float d = b * b - 4 * a * c;

    if (d <= EPSILON) {
        return NO_HIT;
    }

    float sd = sqrt(d);
    float t1 = (-b - sd) / (2 * a);
    float t2 = (-b + sd) / (2 * a);

    float t  = t1 > EPSILON * 10 ? t1 : t2;
    t        = t > EPSILON * 10 ? t : -1;
    vec3 pos = ro + rd * t;
    if (sign(t1) != sign(t2)) {
        return Hit(t, -normalize(pos - center), true);
    } else {
        return Hit(t, normalize(pos - center), false);
    }
}

// Stolen from https://www.shadertoy.com/view/4lcSRn
Hit intersectCylinder(vec3 ro, vec3 rd, vec3 center, vec3 direction, float radius, float height)
{
    float ra = radius;
    vec3 a   = center;
    vec3 b   = center + height * direction;

    vec3 ba    = b - a;
    vec3 oc    = ro - a;
    float baba = dot(ba, ba);
    float bard = dot(ba, rd);
    float baoc = dot(ba, oc);
    float k2   = baba - bard * bard;
    float k1   = baba * dot(oc, rd) - baoc * bard;
    float k0   = baba * dot(oc, oc) - baoc * baoc - ra * ra * baba;
    float h    = k1 * k1 - k2 * k0;
    if (h < 0.0)
        return NO_HIT; // no intersection
    h = sqrt(h);

    float t1 = (-k1 - h) / k2;
    float y1 = baoc + t1 * bard;
    float t2 = (-k1 + h) / k2;
    float y2 = baoc + t2 * bard;

    bool good1 = y1 > 0.0 && y1 < baba;
    bool good2 = y2 > 0.0 && y2 < baba;

    if (good1) {
        return Hit(t1, (oc + t1 * rd - ba * y1 / baba) / ra, false);
    }
    if (good2) {
        return Hit(t2, (oc + t2 * rd - ba * y2 / baba) / ra, true);
    }
    return NO_HIT; // no intersection
}


// plane degined by p (p.xyz must be normalized)
Hit intersectDisc(vec3 ro, vec3 rd, vec3 normal, vec3 center, float radius)
{
    vec3 o  = ro - center;
    float t = -dot(normal, o) / dot(rd, normal);
    vec3 q  = o + rd * t;

    float dist = length(q);

    t = t > EPSILON && dist < radius ? t : -1;
    return Hit(t, normal, false);
}


vec2 cap(vec3 rd, vec3 ro, vec3 pos, vec3 normal, float radius)
{
    vec3 o     = ro - pos;
    float t    = -dot(normal, o) / dot(rd, normal);
    vec3 q     = o + rd * t;
    float dist = length(q);
    bool pass  = t > EPSILON && dist < radius;
    return vec2(t, float(pass));
}


Hit intersectCappedCylinder(vec3 ro, vec3 rd, vec3 center, vec3 direction, float radius, float height)
{
    float ra = radius;
    vec3 a   = center;
    vec3 b   = center + height * direction;

    vec3 ba    = b - a;
    vec3 oc    = ro - a;
    float baba = dot(ba, ba);
    float bard = dot(ba, rd);
    float baoc = dot(ba, oc);
    float k2   = baba - bard * bard;
    float k1   = baba * dot(oc, rd) - baoc * bard;
    float k0   = baba * dot(oc, oc) - baoc * baoc - ra * ra * baba;
    float h    = k1 * k1 - k2 * k0;
    if (h < 0.0)
        return NO_HIT; // no intersection
    h = sqrt(h);

    float t1 = (-k1 - h) / k2;
    float y1 = baoc + t1 * bard;
    float t2 = (-k1 + h) / k2;
    float y2 = baoc + t2 * bard;

    bool good1 = y1 > 0.0 && y1 < baba && t1 > EPSILON;
    bool good2 = y2 > 0.0 && y2 < baba;

    vec2 tc1 = cap(rd, ro, a, -direction, ra);
    vec2 tc2 = cap(rd, ro, b, direction, ra);

    float[2] t_caps   = { tc1.x, tc2.x };
    bool[2] pass_caps = { tc1.y > EPSILON, tc2.y > EPSILON };
    vec3[2] n_caps    = { -direction, direction };

    int closerCap  = tc1.x <= tc2.x ? 0 : 1;
    int furtherCap = closerCap == 1 ? 0 : 1;


    if (good1) {
        return Hit(t1, (oc + t1 * rd - ba * y1 / baba) / ra, false);
    }
    if (pass_caps[closerCap]) {
        return Hit(t_caps[closerCap], n_caps[closerCap], false);
    }
    if (good2) {
        return Hit(t2, -(oc + t2 * rd - ba * y2 / baba) / ra, true);
    }
    if (pass_caps[furtherCap]) {
        return Hit(t_caps[furtherCap], -n_caps[furtherCap], true);
    }

    return NO_HIT; // no intersection
}


Hit intersectUpperCappedCylinder(vec3 ro, vec3 rd, vec3 center, vec3 direction, float radius, float height)
{
    float ra = radius;
    vec3 a   = center;
    vec3 b   = center + height * direction;

    vec3 ba    = b - a;
    vec3 oc    = ro - a;
    float baba = dot(ba, ba);
    float bard = dot(ba, rd);
    float baoc = dot(ba, oc);
    float k2   = baba - bard * bard;
    float k1   = baba * dot(oc, rd) - baoc * bard;
    float k0   = baba * dot(oc, oc) - baoc * baoc - ra * ra * baba;
    float h    = k1 * k1 - k2 * k0;
    if (h < 0.0)
        return NO_HIT; // no intersection
    h = sqrt(h);

    float t1 = (-k1 - h) / k2;
    float y1 = baoc + t1 * bard;
    float t2 = (-k1 + h) / k2;
    float y2 = baoc + t2 * bard;

    bool good1 = y1 > 0.0 && y1 < baba && t1 > EPSILON;
    bool good2 = y2 > 0.0 && y2 < baba;

    vec2 tc1 = cap(rd, ro, a, -direction, ra);
    vec2 tc2 = cap(rd, ro, b, direction, ra);

    float[2] t_caps   = { tc1.x, tc2.x };
    bool[2] pass_caps = { tc1.y > EPSILON, tc2.y > EPSILON };
    vec3[2] n_caps    = { -direction, direction };

    int closerCap  = tc1.x <= tc2.x ? 0 : 1;
    int furtherCap = closerCap == 1 ? 0 : 1;


    if (good1) {
        return Hit(t1, (oc + t1 * rd - ba * y1 / baba) / ra, false);
    }
    if (pass_caps[closerCap]) {
        return Hit(t_caps[closerCap], n_caps[closerCap], false);
    }
    if (good2) {
        return Hit(t2, -(oc + t2 * rd - ba * y2 / baba) / ra, true);
    }

    return NO_HIT; // no intersection
}


// plane degined by p (p.xyz must be normalized)
Hit intersectPlane(vec3 ro, vec3 rd, vec3 center, vec3 normal, vec3 direction, float size)
{
    vec3 o  = ro - center;
    float t = -dot(normal, o) / dot(rd, normal);
    vec3 q  = o + rd * t;

    vec3 d1    = direction;
    vec3 d2    = cross(normal, d1);
    float dist = max(abs(dot(q, d1)), abs(dot(q, d2)));

    t = t > EPSILON && dist < size ? t : -1;
    return Hit(t, normal, false);
}


// #define TRACE_N_OBJECTS(n)                                                                                                                           \
//     HitInfo traceObjects##n(vec3 ro, vec3 rd, RenderItem items[n])                                                                                   \
//     {                                                                                                                                                \
//         float min_t  = 1000000;                                                                                                                      \
//         int hitIndex = 0;                                                                                                                            \
//         Hit result   = NO_HIT;                                                                                                                       \
//         Material material;                                                                                                                           \
//         bool isLight;                                                                                                                                \
//         for (int i = 0; i < n; ++i) {                                                                                                                \
//             Hit hit = traceIntersect(ro, rd, items[i]);                                                                                              \
//             if (hit.t < EPSILON) {                                                                                                                   \
//                 continue;                                                                                                                            \
//             }                                                                                                                                        \
//             if (hit.t < min_t) {                                                                                                                     \
//                 min_t    = hit.t;                                                                                                                    \
//                 result   = hit;                                                                                                                      \
//                 material = items[i].material;                                                                                                        \
//                 isLight  = items[i].type == TYPE_POINT_LIGHT;                                                                                        \
//                 hitIndex = i;                                                                                                                        \
//             }                                                                                                                                        \
//         }                                                                                                                                            \
//         return HitInfo(result, -rd, material, isLight);                                                                                              \
//     }

// TRACE_N_OBJECTS(3)
// TRACE_N_OBJECTS(4)

#define FIND_CLOSEST_HIT_FN(array_len)                                                                                                               \
    Hit findClosestHit(Hit hits[array_len])                                                                                                          \
    {                                                                                                                                                \
        float min_t  = 1000000;                                                                                                                      \
        int hitIndex = -1;                                                                                                                           \
        for (int i = 0; i < array_len; ++i) {                                                                                                        \
            if (hits[i].t < EPSILON) {                                                                                                               \
                continue;                                                                                                                            \
            }                                                                                                                                        \
            if (hits[i].t < min_t) {                                                                                                                 \
                min_t    = hits[i].t;                                                                                                                \
                hitIndex = i;                                                                                                                        \
            }                                                                                                                                        \
        }                                                                                                                                            \
        if (hitIndex == -1) {                                                                                                                        \
            return NO_HIT;                                                                                                                           \
        } else {                                                                                                                                     \
            return hits[hitIndex];                                                                                                                   \
        }                                                                                                                                            \
    }

FIND_CLOSEST_HIT_FN(2)
FIND_CLOSEST_HIT_FN(3)
FIND_CLOSEST_HIT_FN(4)
FIND_CLOSEST_HIT_FN(5)

HitInfo intersectPawn(vec3 ro, vec3 rd, float x, float z, int color)
{
    Hit hits[3];

    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.0001, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.1, 0.9);
    hits[2] = intersectSphere(ro, rd, vec3(x, 0.9, z), 0.2);

    return HitInfo(findClosestHit(hits), -rd, materialRed, false);
}

HitInfo intersectRook(vec3 ro, vec3 rd, float x, float z, int color)
{
    Hit hits[4];
    hits[0] = intersectCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.4, 0.1);
    hits[1] = intersectCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.32, 0.17);
    hits[2] = intersectCylinder(ro, rd, vec3(x, 0.17, z), vec3(0, 1, 0), 0.2, 0.6);
    hits[3] = intersectCylinder(ro, rd, vec3(x, 0.6, z), vec3(0, 1, 0), 0.25, 0.171);

    return HitInfo(findClosestHit(hits), -rd, materialRed, false);
}

HitInfo intersectKnight(vec3 ro, vec3 rd, float x, float z, int color)
{
    Hit hits[5];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.2, 0.06);
    hits[2] = intersectCappedCylinder(ro, rd, vec3(x, 0.1, z), normalize(vec3(1, 2, 0)), 0.1, 0.6);
    hits[3] = intersectCappedCylinder(ro, rd, vec3(x - 0.2, 0.5, z), normalize(vec3(1, 0.5, 0)), 0.1, 0.3);
    hits[4] = intersectSphere(ro, rd, vec3(x + 0.2, 0.7, z), 0.2);

    return HitInfo(findClosestHit(hits), -rd, materialRed, false);
}

HitInfo intersectBishop(vec3 ro, vec3 rd, float x, float z, int color)
{
    Hit hits[4];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.14, 0.5);
    hits[2] = intersectSphere(ro, rd, vec3(x, 0.7, z), 0.2);
    hits[3] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.85, z), vec3(0, 1, 0), 0.05, 0.1);

    return HitInfo(findClosestHit(hits), -rd, materialRed, false);
}

HitInfo intersectKing(vec3 ro, vec3 rd, float x, float z, int color)
{
    Hit hits[4];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.2, 0.6);
    hits[2] = intersectSphere(ro, rd, vec3(x, 0.7, z), 0.2);
    hits[3] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.85, z), vec3(0, 1, 0), 0.05, 0.1);

    return HitInfo(findClosestHit(hits), -rd, materialRed, false);
}

HitInfo intersectFigure(vec3 ro, vec3 rd, int figure_type, int color, float x, float z)
{
    switch (figure_type) {
        case FIGURE_PAWN: {
            return intersectPawn(ro, rd, x, z, color);
        }
        case FIGURE_ROOK: {
            return intersectRook(ro, rd, x, z, color);
        }
        case FIGURE_KNIGHT: {
            return intersectKnight(ro, rd, x, z, color);
        }
        case FIGURE_BISHOP: {
            return intersectBishop(ro, rd, x, z, color);
        }
        case FIGURE_KING: {
            return intersectKing(ro, rd, x, z, color);
        }

        default:
            return HitInfo(NO_HIT, -rd, materialRed, false);
    }
}

HitInfo traceRay(vec3 ro, vec3 rd)
{
    float x = 0;
    float z = 2;


    const int itemCount = 2;

    Hit hits[itemCount];
    hits[0] = intersectSphere(ro, rd, light.position, light.radius);
    hits[1] = intersectPlane(ro, rd, vec3(-0.5, 0, -0.5), vec3(0, 1, 0), vec3(1, 0, 0), 4);

    Material materials[itemCount];
    materials[0] = materialWhite;
    materials[1] = materialGray;

    bool isLights[itemCount];
    isLights[0] = true;
    isLights[1] = false;

    Hit result = NO_HIT;
    Material material;
    bool isLight = false;

    float min_t  = 1000000;
    int hitIndex = -1;

    for (int i = 0; i < itemCount; ++i) {
        if (hits[i].t < EPSILON) {
            continue;
        }

        if (hits[i].t < min_t) {
            min_t    = hits[i].t;
            result   = hits[i];
            material = materials[i];
            isLight  = isLights[i];
            hitIndex = i;
        }
    }

    if (hitIndex == itemCount - 1) {
        vec4 newColor = vec4(0, 0, 0, 1);
        vec3 pos      = ro + rd * result.t;
        if (int(floor(pos.x + 0.5)) % 2 == int(floor(pos.z + 0.5)) % 2) {
            newColor = vec4(1, 1, 1, 1);
        }
        material.color = newColor.xyz;
    }

    HitInfo figureHit = intersectFigure(ro, rd, FIGURE_KING, COLOR_WHITE, x, z);

    if (figureHit.hit.t > EPSILON && figureHit.hit.t < result.t || result.t < EPSILON) {
        result   = figureHit.hit;
        material = figureHit.material;
        isLight  = figureHit.isLight;
    }

    return HitInfo(result, -rd, material, isLight);
}

// https://www.shadertoy.com/view/XsXXDB
vec3 shadeBlinnPhong(HitInfo hitInfo, vec3 pos, LightItem light)
{
    float diffuse  = 0.6;
    float specular = 0.4;
    vec3 n         = hitInfo.hit.normal;

    vec3 res = vec3(0.);
    vec3 ld  = normalize(light.position - pos);
    res      = hitInfo.material.color * diffuse * dot(n, ld);
    vec3 h   = normalize(hitInfo.ed + ld);
    res += specular * pow(dot(n, h), 16.);
    res = clamp(res, 0., 1.);

    return res;
}

// https://www.shadertoy.com/view/XsXXDB
vec3 shadeCookTorrance(HitInfo hitInfo, vec3 pos, LightItem light)
{
    float roughness = hitInfo.material.roughness;
    float K         = hitInfo.material.density;
    //
    vec3 ld     = normalize(light.position - pos);
    vec3 h      = normalize(hitInfo.ed + ld);
    float NdotL = clamp(dot(hitInfo.hit.normal, ld), 0., 1.);
    float NdotH = clamp(dot(hitInfo.hit.normal, h), 0., 1.);
    float NdotV = clamp(dot(hitInfo.hit.normal, hitInfo.ed), 0., 1.);
    float VdotH = clamp(dot(h, hitInfo.ed), 0., 1.);
    float rsq   = roughness * roughness;

    // Geometric Attenuation
    float NH2   = 2. * NdotH / VdotH;
    float geo_b = (NH2 * NdotV);
    float geo_c = (NH2 * NdotL);
    float geo   = min(1., min(geo_b, geo_c));

    // Roughness
    // Beckmann distribution function
    float r1    = 1. / (4. * rsq * pow(NdotH, 4.));
    float r2    = (NdotH * NdotH - 1.) / (rsq * NdotH * NdotH);
    float rough = r1 * exp(r2);

    // Fresnel
    float n = hitInfo.material.n;
    // Schlick's approximation
    float F0   = pow(n - 1., 2) / pow(n + 1., 2);
    float fres = pow(1.0 - VdotH, 5.);
    fres *= (1.0 - F0);
    fres += F0;

    vec3 spec = (NdotV * NdotL == 0.) ? vec3(0.) : vec3(fres * geo * rough) / (NdotV * NdotL);
    vec3 res  = NdotL * ((1. - K) * spec + K * hitInfo.material.color) * light.material.color;

    return res;
}

vec3 shade(HitInfo hitInfo, vec3 pos)
{
    float frac     = 1. / float(LIGHT_COUNT);
    vec3 res_color = vec3(0.);

    for (int light_i = 0; light_i < LIGHT_COUNT; light_i++) {
        LightItem light = lights[light_i];
        // vec3 color = shadeBlinnPhong(hitInfo, light);
        if (hitInfo.isLight) {
            return hitInfo.material.color;
        }

        vec3 color = shadeCookTorrance(hitInfo, pos, light);
        res_color += color * frac;
    }
    return res_color;
}

// https://www.shadertoy.com/view/4djSRW
float hash12(vec2 p)
{
    vec3 p3 = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}


vec2 randomSampleUnitCircle(vec2 st)
{
    float seed  = hash12(st);
    float r     = sqrt(seed);
    float seed2 = hash12(vec2(st.y, st.x));
    float theta = 6.28318530718 * seed2;
    return vec2(r, theta);
}


float radius(int i, int n, float b)
{
    if (i >= n - b) {
        return 1;
    } else {
        return sqrt(float(i) / float(n - (b + 1) / 2));
    }
}


vec2 sunflower(int n, int alpha, int i)
{
    float b     = round(alpha * sqrt(n));
    float r     = radius(i, n, b);
    float theta = 2 * PI * (i + 1) / pow(GOLDEN_RATIO, 2);
    return vec2(r, theta);
}


float calculateShadow(vec3 pos)
{
    float frac       = 1. / float(LIGHT_COUNT);
    float res_shadow = 0.;

    for (int light_i = 0; light_i < LIGHT_COUNT; light_i++) {
        LightItem light = lights[light_i];

        vec3 lightDir      = normalize(light.position - pos);
        vec3 perpLightDir1 = normalize(cross(lightDir, vec3(lightDir.x + 1, lightDir.y + 1, lightDir.z - 1)));
        vec3 perpLightDir2 = normalize(cross(lightDir, perpLightDir1));
        vec2 seed          = gl_FragCoord.xy;

        float shadow = 0;

        for (int i = 0; i < shadowRays; i++) {
            vec2 rsample = randomSampleUnitCircle(seed);
            // vec2 rsample = sunflower(shadowRays, 1, i);
            float r     = rsample.x * light.radius;
            float theta = rsample.y;
            float x     = r * cos(theta);
            float y     = r * sin(theta);

            vec3 offset = perpLightDir1 * x + perpLightDir2 * y;
            vec3 rayDir = normalize(light.position + offset - pos);

            HitInfo shadowHit = traceRay(pos + 0.001 * lightDir, rayDir);
            vec3 shadowHitPos = pos + shadowHit.hit.t * rayDir;
            bool res          = shadowHit.hit.t > EPSILON && shadowHit.hit.t < (length(light.position - shadowHitPos) - light.radius);

            shadow += float(res);

            float v  = float(i + 1) * .152;
            vec2 pos = (gl_FragCoord.xy * v + 50.0);
            seed     = gl_FragCoord.xy * pos;
        }
        res_shadow += (shadow / float(shadowRays)) * frac;
    }

    return res_shadow;
}

float calculateShadowHard(vec3 pos)
{
    float frac       = 1. / float(LIGHT_COUNT);
    float res_shadow = 0.;

    for (int light_i = 0; light_i < LIGHT_COUNT; light_i++) {
        LightItem light = lights[light_i];
        vec3 lightDir   = normalize(light.position - pos);

        HitInfo shadowHit = traceRay(pos + 0.001 * lightDir, lightDir);
        vec3 shadowHitPos = pos + shadowHit.hit.t * lightDir;
        bool res          = shadowHit.hit.t > EPSILON && shadowHit.hit.t < (length(light.position - shadowHitPos) - light.radius);

        res_shadow += float(res) * frac;
    }

    return res_shadow;
}

void whatColorIsThere(vec3 ro, vec3 rd)
{
    HitInfo hitInfo = traceRay(ro, rd);
    Hit hit         = hitInfo.hit;

    if (hitInfo.isLight) {
        fSpecDiff = vec4(hitInfo.material.color, 1);
        fAmbient  = vec4(hitInfo.material.color, 1);
        fShadow   = 0.;
        return;
    }

    if (hit.t > EPSILON) {
        vec3 primaryPos = ro + rd * hit.t;

        float primaryShadow = calculateShadow(primaryPos + hit.normal * 0.001);
        primaryShadow *= 1 - hitInfo.material.transparency;

        vec3 color        = shade(hitInfo, primaryPos);
        vec3 primaryColor = hitInfo.material.color;

        HitInfo currentHit = hitInfo;
        vec3 currentRo     = primaryPos;
        vec3 currentRd     = reflect(rd, currentHit.hit.normal);
        currentRo += 0.0001 * currentRd;
        float refl = 1;
        float d    = currentHit.material.density;
        float dinv = 1. - d;
        float t    = currentHit.material.transparency;
        float tinv = 1. - t;
        vec3 accum = color * d * tinv;

        float n1, n2;
        if (!hitInfo.hit.inside) {
            n1 = 1.;
            n2 = hitInfo.material.n;
        } else {
            n1 = hitInfo.material.n;
            n2 = 1.;
        }

        // determine how much light is reflected and how much is refracted
        float incident_angle  = acos(dot(-rd, hitInfo.hit.normal));
        float refracted_angle = asin(n1 / n2 * sin(incident_angle));

        float rcf_r             = (n1 * cos(incident_angle) - n2 * cos(refracted_angle)) / (n1 * cos(incident_angle) + n2 * cos(refracted_angle));
        float reflection_coef   = rcf_r * rcf_r;
        float transmission_coef = 1 - reflection_coef;

        float critical_angle = asin(n2 / n1);
        if (incident_angle > critical_angle) {
            reflection_coef   = 1;
            transmission_coef = 0;
        }

        transmission_coef *= t;
        reflection_coef = 1 - transmission_coef;

        vec3 refl_accum   = vec3(0);
        float refl_shadow = 1;
        vec3 pos          = primaryPos;
        // handle reflections
        for (int k = 1; k < 4; ++k) {
            refl *= 1. - currentHit.material.density;
            if (refl < 0.01)
                break;

            currentHit = traceRay(currentRo, currentRd);
            if (currentHit.hit.t < EPSILON)
                break;

            pos = currentRo + currentHit.hit.t * currentRd;
            refl_shadow *= calculateShadowHard(pos);

            vec3 color = shade(currentHit, pos);

            refl_accum += color * refl;

            currentRo = pos;
            currentRd = reflect(currentRd, currentHit.hit.normal);
            currentRo += 0.0001 * currentRd;
        }

        // handle refraction
        currentHit = hitInfo; // li
        currentRo  = primaryPos; // lro

        float n;
        if (hitInfo.hit.inside) {
            n = hitInfo.material.n;
        } else {
            n = 1. / hitInfo.material.n;
        }

        currentRd = refract(rd, currentHit.hit.normal, n); // lrd
        currentRo += 0.0001 * -currentHit.hit.normal;
        float refr = 1;

        float[20] refr_stack;
        refr_stack[0]   = 1.;
        refr_stack[1]   = currentHit.material.n;
        int stack_index = currentHit.hit.inside ? 0 : 1;

        vec3 refr_accum   = vec3(0);
        float refr_shadow = 0;
        for (int k = 1; k < 6; ++k) {
            refr *= currentHit.material.transparency;
            currentHit = traceRay(currentRo, currentRd);
            if (currentHit.hit.t < EPSILON)
                break;

            pos = currentRo + currentHit.hit.t * currentRd;
            if (!currentHit.hit.inside) {
                refr_shadow += calculateShadow(pos) * (1 - currentHit.material.transparency);
                n = refr_stack[stack_index] / currentHit.material.n;
                stack_index++;
                refr_stack[stack_index] = currentHit.material.n;
            } else {
                stack_index--;
                stack_index = max(stack_index, 0);
                n           = currentHit.material.n / refr_stack[stack_index];
            }

            vec3 color = shade(currentHit, pos);
            refr_accum += color * refr;

            currentRd = refract(currentRd, currentHit.hit.normal, n); // lrd
            currentRo = pos + 0.0001 * -currentHit.hit.normal;

            if (refr < 0.0001)
                break;
        }

        accum += refr_accum * transmission_coef + refl_accum * reflection_coef * refl_shadow * dinv;
        // accum *= primaryShadow;

        fSpecDiff = vec4(accum, 1);
        // fSpecDiff = vec4(refl_shadow, 0, 0, 1);
        // fSpecDiff = vec4(refl_accum,1.);
        fAmbient = vec4(primaryColor * vec3(0.1), 1);

        // fSpecDiff = max(fSpecDiff, fAmbient);
        fShadow = primaryShadow + refr_shadow;
        // fShadow =  primaryShadow;

    } else {
        fSpecDiff = vec4(0, 0, 0, 1);
        fAmbient  = vec4(0, 0, 0, 1);
        fShadow   = 0;
    }
}

void main()
{
    float bigger  = screenWidth > screenHeight ? screenWidth : screenHeight;
    float waspect = screenWidth / bigger;
    float haspect = screenHeight / bigger;

    vec3 ro = (cameraMatrix * vec4(ORIGIN, 1)).xyz;
    vec3 re = (cameraMatrix * vec4(vec3(gl_FragCoord.x / bigger - 0.5 * waspect, gl_FragCoord.y / bigger - 0.5 * haspect, 1), 1)).xyz;

    vec3 rd = normalize(re - ro);

    whatColorIsThere(ro, rd);
}

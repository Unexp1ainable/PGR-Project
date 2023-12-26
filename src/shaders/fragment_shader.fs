#version 460
layout(location = 0) out float fShadow;
layout(location = 1) out vec4 fSpecDiff;
layout(location = 2) out vec4 fAmbient;

in vec4 gl_FragCoord;
uniform samplerCube skybox;


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


#define FIGURE_EMPTY  0
#define FIGURE_PAWN   1
#define FIGURE_KING   2
#define FIGURE_QUEEN  3
#define FIGURE_BISHOP 4
#define FIGURE_KNIGHT 5
#define FIGURE_ROOK   6

#define COLOR_NONE  0
#define COLOR_WHITE 1
#define COLOR_BLACK 2

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
    int materialId;
    bool isLight;
};

uniform int screenWidth       = 1024;
uniform int screenHeight      = 768;
uniform mat4 cameraMatrix     = mat4(1.0);
uniform int reflectionBounces = 3;
uniform int shadowRays        = 1;
uniform vec3 lightPosition    = vec3(0, 4, 0);
uniform float n               = 1.2;
uniform uint time             = 42;
uniform uint accumCounter = 1;

uniform float roughness    = 0.68;
uniform float transparency = 0.5;
uniform float density      = 0.8;
uniform int chessBoard[8*8*2];
//  = {
//         {{FIGURE_ROOK, COLOR_WHITE}, {FIGURE_KNIGHT, COLOR_WHITE}, {FIGURE_BISHOP, COLOR_WHITE}, {FIGURE_QUEEN, COLOR_WHITE}, {FIGURE_KING, COLOR_WHITE}, {FIGURE_BISHOP, COLOR_WHITE}, {FIGURE_KNIGHT, COLOR_WHITE}, {FIGURE_ROOK, COLOR_WHITE}},
//         {{FIGURE_PAWN, COLOR_WHITE}, {FIGURE_PAWN, COLOR_WHITE}, {FIGURE_PAWN, COLOR_WHITE}, {FIGURE_PAWN, COLOR_WHITE}, {FIGURE_PAWN, COLOR_WHITE}, {FIGURE_PAWN, COLOR_WHITE}, {FIGURE_PAWN, COLOR_WHITE}, {FIGURE_PAWN, COLOR_WHITE}},
//         {{FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}},
//         {{FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}},
//         {{FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}},
//         {{FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}, {FIGURE_EMPTY, COLOR_NONE}},
//         {{FIGURE_PAWN, COLOR_BLACK}, {FIGURE_PAWN, COLOR_BLACK}, {FIGURE_PAWN, COLOR_BLACK}, {FIGURE_PAWN, COLOR_BLACK}, {FIGURE_PAWN, COLOR_BLACK}, {FIGURE_PAWN, COLOR_BLACK}, {FIGURE_PAWN, COLOR_BLACK}, {FIGURE_PAWN, COLOR_BLACK}},
//         {{FIGURE_ROOK, COLOR_BLACK}, {FIGURE_KNIGHT, COLOR_BLACK}, {FIGURE_BISHOP, COLOR_BLACK}, {FIGURE_QUEEN, COLOR_BLACK}, {FIGURE_KING, COLOR_BLACK}, {FIGURE_BISHOP, COLOR_BLACK}, {FIGURE_KNIGHT, COLOR_BLACK}, {FIGURE_ROOK, COLOR_BLACK}}
//     };
//  = {
    
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
//         {{ FIGURE_ROOK, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_QUEEN, COLOR_WHITE }, { FIGURE_KING, COLOR_WHITE }, { FIGURE_BISHOP, COLOR_WHITE }, { FIGURE_KNIGHT, COLOR_WHITE }, { FIGURE_ROOK, COLOR_WHITE } },
    
// };

Material materialRed   = Material(vec3(1, 0, 0), n, roughness, transparency, density);
Material materialGreen = Material(vec3(0, 1, 0), 1.4, roughness, transparency, density);
Material materialBlue  = Material(vec3(0, 0, 1), 2.5, roughness, transparency, density);
Material materialGray  = Material(vec3(0.2, 0.2, 0.2), 1., roughness, 0., 1.);
Material materialWhite = Material(vec3(1, 1, 1), 1., roughness, 0., 1.);
Material materialBlack = Material(vec3(0, 0, 0), 1., roughness, 0., 1.);

Material materials[4] = { 
    materialRed, 
    materialGray, 
    materialWhite,
    materialBlack
    };

#define MATERIAL_RED   0
#define MATERIAL_GRAY  1
#define MATERIAL_WHITE 2
#define MATERIAL_BLACK 3

struct LightItem {
    vec3 position;
    float radius;
    int materialId;
};

LightItem light = LightItem(lightPosition, 0.5, MATERIAL_WHITE);

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

// axis aligned box centered at the origin, with size boxSize
Hit boxIntersection(in vec3 ro, in vec3 rd, vec3 pos, vec3 boxSize)
{
    vec3 wro = ro - pos; // ro relative to box center

    vec3 m   = 1.0 / rd; // can precompute if traversing a set of aligned boxes
    vec3 n   = m * wro; // can precompute if traversing a set of aligned boxes
    vec3 k   = abs(m) * boxSize;
    vec3 t1  = -n - k;
    vec3 t2  = -n + k;
    float tN = max(max(t1.x, t1.y), t1.z);
    float tF = min(min(t2.x, t2.y), t2.z);
    if (tN > tF || tF < 0.0)
        return NO_HIT; // no intersection
    vec3 outNormal = (tN > 0.0) ? step(vec3(tN), t1) : // ro ouside the box
        step(t2, vec3(tF)); // ro inside the box
    outNormal *= -sign(rd);

    if (tN < 0.0) {
        tN = tF;
    }
    return Hit(tN, outNormal, false);
}

bool boxIntersectionFast(in vec3 ro, in vec3 rd, vec3 pos, vec3 boxSize)
{
    vec3 wro = ro - pos; // ro relative to box center
    vec3 m   = 1.0 / rd;
    vec3 n   = m * wro;
    vec3 k   = abs(m) * boxSize;
    vec3 t1  = -n - k;
    vec3 t2  = -n + k;
    float tN = max(max(t1.x, t1.y), t1.z);
    float tF = min(min(t2.x, t2.y), t2.z);
    return !(tN > tF || tF < 0.0);
}


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
FIND_CLOSEST_HIT_FN(6)

Hit intersectPawn(vec3 ro, vec3 rd, float x, float z)
{
    if (!boxIntersectionFast(ro, rd, vec3(x, 0.55, z), vec3(0.3, 0.55, 0.3))) {
        return NO_HIT;
    }

    Hit hits[3];

    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.0001, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.1, 0.9);
    hits[2] = intersectSphere(ro, rd, vec3(x, 0.9, z), 0.2);

    return findClosestHit(hits);
}

Hit intersectRook(vec3 ro, vec3 rd, float x, float z)
{
    if (!boxIntersectionFast(ro, rd, vec3(x, 0.4, z), vec3(0.4, 0.4, 0.4))) {
        return NO_HIT;
    }


    Hit hits[4];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.4, 0.1);
    hits[1] = intersectCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.32, 0.17);
    hits[2] = intersectCappedCylinder(ro, rd, vec3(x, 0.17, z), vec3(0, 1, 0), 0.2, 0.6);
    hits[3] = intersectCappedCylinder(ro, rd, vec3(x, 0.6, z), vec3(0, 1, 0), 0.25, 0.171);

    return findClosestHit(hits);
}

Hit intersectKnight(vec3 ro, vec3 rd, float x, float z, int color)
{
    float r = color != COLOR_WHITE ? 1. : -1;

    if (!boxIntersectionFast(ro, rd, vec3(x, 0.4, z + 0.05 * r), vec3(0.3, 0.5, 0.35))) {
        return NO_HIT;
    }

    Hit hits[5];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.2, 0.06);
    hits[2] = intersectCappedCylinder(ro, rd, vec3(x, 0.1, z), normalize(vec3(0, 2, 1 * r)), 0.1, 0.6);
    hits[3] = intersectCappedCylinder(ro, rd, vec3(x, 0.5, z - 0.2 * r), normalize(vec3(0, 0.5, 1 * r)), 0.1, 0.3);
    hits[4] = intersectSphere(ro, rd, vec3(x, 0.7, z + 0.2 * r), 0.2);

    return findClosestHit(hits);
}

Hit intersectBishop(vec3 ro, vec3 rd, float x, float z)
{
    if (!boxIntersectionFast(ro, rd, vec3(x, 0.4751, z), vec3(0.3, 0.4751, 0.3))) {
        return NO_HIT;
    }

    Hit hits[4];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.14, 0.5);
    hits[2] = intersectSphere(ro, rd, vec3(x, 0.7, z), 0.2);
    hits[3] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.85, z), vec3(0, 1, 0), 0.05, 0.1);

    return findClosestHit(hits);
}

Hit intersectKing(vec3 ro, vec3 rd, float x, float z)
{
    if (!boxIntersectionFast(ro, rd, vec3(x, 0.6, z), vec3(0.3, 0.6, 0.3))) {
        return NO_HIT;
    }

    Hit hits[5];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.14, 0.6);
    hits[2] = intersectSphere(ro, rd, vec3(x, 0.8, z), 0.2);
    hits[3] = intersectSphere(ro, rd, vec3(x, 1., z), 0.1);
    hits[4] = intersectUpperCappedCylinder(ro, rd, vec3(x, 1.07, z), vec3(0, 1, 0), 0.03, 0.1);

    return findClosestHit(hits);
}

Hit intersectQueen(vec3 ro, vec3 rd, float x, float z)
{
    if (!boxIntersectionFast(ro, rd, vec3(x, 0.52, z), vec3(0.3, 0.52, 0.3))) {
        return NO_HIT;
    }

    Hit hits[5];
    hits[0] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0, z), vec3(0, 1, 0), 0.3, 0.1);
    hits[1] = intersectUpperCappedCylinder(ro, rd, vec3(x, 0.1, z), vec3(0, 1, 0), 0.1, 0.6);
    hits[2] = intersectSphere(ro, rd, vec3(x, 0.8, z), 0.15);
    hits[3] = intersectCappedCylinder(ro, rd, vec3(x, 0.8, z), vec3(0, 1, 0), 0.15, 0.2);
    hits[4] = intersectSphere(ro, rd, vec3(x, 1, z), 0.04);

    return findClosestHit(hits);
}


Hit intersectFigure(vec3 ro, vec3 rd, int figure_type, int color, float x, float z)
{
    switch (figure_type) {
        case FIGURE_PAWN: {
            return intersectPawn(ro, rd, x, z);
        }
        case FIGURE_ROOK: {
            return intersectRook(ro, rd, x, z);
        }
        case FIGURE_KNIGHT: {
            return intersectKnight(ro, rd, x, z, color);
        }
        case FIGURE_BISHOP: {
            return intersectBishop(ro, rd, x, z);
        }
        case FIGURE_KING: {
            return intersectKing(ro, rd, x, z);
        }
        case FIGURE_QUEEN: {
            return intersectQueen(ro, rd, x, z);
        }
        default:
            return NO_HIT;
    }

    return NO_HIT;
}

HitInfo intersectFigures(vec3 ro, vec3 rd)
{
    // return intersectFigure(ro, rd, FIGURE_BISHOP, COLOR_WHITE, 0, 0);
    // return intersectBishop(ro, rd, 0, 0, 0);
    float min_t    = 1000000;
    Hit result = NO_HIT;
    int matId = 0;

    for (int row = 0; row < 8; row++) {
        for (int col = 0; col < 8; col++) {
            int figure_type = chessBoard[(row*8 + col)*2];
            int color       = chessBoard[(row*8 + col)*2+1];
            float x         = col - 4;
            float z         = row - 4;
            Hit hit = intersectFigure(ro, rd, figure_type, color, x, z);
            if (hit.t > EPSILON && hit.t < min_t) {
                min_t  = hit.t;
                result = hit;
                matId = color == COLOR_WHITE ? MATERIAL_RED : MATERIAL_GRAY;
            }
        }
    }

    return HitInfo(result, matId, false);
}

HitInfo traceRay(vec3 ro, vec3 rd)
{
    float x = 0;
    float z = 2;


    const int itemCount = 2;

    Hit hits[itemCount];
    hits[0] = intersectSphere(ro, rd, light.position, light.radius);
    hits[1] = intersectPlane(ro, rd, vec3(-0.5, 0, -0.5), vec3(0, 1, 0), vec3(1, 0, 0), 4);

    int materials[itemCount];
    materials[0] = MATERIAL_WHITE;
    materials[1] = MATERIAL_GRAY;

    bool isLights[itemCount];
    isLights[0] = true;
    isLights[1] = false;

    Hit result = NO_HIT;
    int matId;
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
            matId = materials[i];
            isLight  = isLights[i];
            hitIndex = i;
        }
    }

    if (hitIndex == itemCount - 1) {
        matId = MATERIAL_WHITE;
        vec3 pos      = ro + rd * result.t;
        if (int(floor(pos.x + 0.5)) % 2 == int(floor(pos.z + 0.5)) % 2) {
            matId = MATERIAL_BLACK;
        }
    }

    HitInfo figureHit = intersectFigures(ro, rd);

    if (figureHit.hit.t > EPSILON && figureHit.hit.t < result.t || result.t < EPSILON) {
        return figureHit;
    }

    return HitInfo(result, matId, isLight);
}

// https://www.shadertoy.com/view/XsXXDB
vec3 shadeBlinnPhong(HitInfo hitInfo, vec3 pos, vec3 ed, LightItem light)
{
    float diffuse  = 0.6;
    float specular = 0.4;
    vec3 n         = hitInfo.hit.normal;

    vec3 res = vec3(0.);
    vec3 ld  = normalize(light.position - pos);
    res      = materials[hitInfo.materialId].color * diffuse * dot(n, ld);
    vec3 h   = normalize(ed + ld);
    res += specular * pow(dot(n, h), 16.);
    res = clamp(res, 0., 1.);

    return res;
}

// https://www.shadertoy.com/view/XsXXDB
vec3 shadeCookTorrance(HitInfo hitInfo, vec3 pos, vec3 ed, LightItem light)
{
    float roughness = materials[hitInfo.materialId].roughness;
    float K         = materials[hitInfo.materialId].density;
    //
    vec3 ld     = normalize(light.position - pos);
    vec3 h      = normalize(ed + ld);
    float NdotL = clamp(dot(hitInfo.hit.normal, ld), 0., 1.);
    float NdotH = clamp(dot(hitInfo.hit.normal, h), 0., 1.);
    float NdotV = clamp(dot(hitInfo.hit.normal, ed), 0., 1.);
    float VdotH = clamp(dot(h, ed), 0., 1.);
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
    float n = materials[hitInfo.materialId].n;
    // Schlick's approximation
    float F0   = pow(n - 1., 2) / pow(n + 1., 2);
    float fres = pow(1.0 - VdotH, 5.);
    fres *= (1.0 - F0);
    fres += F0;

    vec3 spec = (NdotV * NdotL == 0.) ? vec3(0.) : vec3(fres * geo * rough) / (NdotV * NdotL);
    vec3 res  = NdotL * ((1. - K) * spec + K * materials[hitInfo.materialId].color) * materials[hitInfo.materialId].color;

    return res;
}

vec3 shade(HitInfo hitInfo, vec3 pos, vec3 ed)
{
    float frac     = 1. / float(LIGHT_COUNT);
    vec3 res_color = vec3(0.);

    for (int light_i = 0; light_i < LIGHT_COUNT; light_i++) {
        LightItem light = lights[light_i];
        // vec3 color = shadeBlinnPhong(hitInfo, light);
        if (hitInfo.isLight) {
            return materials[hitInfo.materialId].color;
        }

        vec3 color = shadeCookTorrance(hitInfo, pos, ed, light);
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
    st.x += time;
    float seed  = hash12(st);
    float r     = sqrt(seed);
    st.y += time;
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
        fSpecDiff = vec4(materials[hitInfo.materialId].color, 1);
        fAmbient  = vec4(materials[hitInfo.materialId].color, 1);
        fShadow   = 0.;
        return;
    }

    if (hit.t > EPSILON) {
        vec3 primaryPos = ro + rd * hit.t;

        Material primaryMaterial = materials[hitInfo.materialId];
        float primaryShadow = calculateShadow(primaryPos + hit.normal * 0.001);
        primaryShadow *= 1 - primaryMaterial.transparency;

        vec3 color        = shade(hitInfo, primaryPos, -rd);
        vec3 primaryColor = primaryMaterial.color;

        HitInfo currentHit = hitInfo;
        Material material = materials[currentHit.materialId];
        vec3 currentRo     = primaryPos;
        vec3 currentRd     = reflect(rd, currentHit.hit.normal);
        currentRo += 0.0001 * currentRd;
        float refl = 1;
        float d    = material.density;
        float dinv = 1. - d;
        float t    = material.transparency;
        float tinv = 1. - t;
        vec3 accum = color * d * tinv;

        float n1, n2;
        if (!hitInfo.hit.inside) {
            n1 = 1.;
            n2 = primaryMaterial.n;
        } else {
            n1 = primaryMaterial.n;
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
            refl *= 1. - material.density;
            if (refl < 0.01)
                break;

            currentHit = traceRay(currentRo, currentRd);
            Material material = materials[currentHit.materialId];
            if (currentHit.hit.t < EPSILON)
                refl_accum += texture(skybox, currentRd).xyz * refl;
                break;


            pos = currentRo + currentHit.hit.t * currentRd;
            refl_shadow *= calculateShadowHard(pos);

            vec3 color = shade(currentHit, pos, -rd);

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
            n = primaryMaterial.n;
        } else {
            n = 1. / primaryMaterial.n;
        }

        currentRd = refract(rd, currentHit.hit.normal, n); // lrd
        currentRo += 0.0001 * -currentHit.hit.normal;
        float refr = 1* material.transparency;

        float[20] refr_stack;
        refr_stack[0]   = 1.;
        refr_stack[1]   = primaryMaterial.n;
        int stack_index = currentHit.hit.inside ? 0 : 1;

        vec3 refr_accum   = vec3(0);
        float refr_shadow = 0;
        for (int k = 1; k < 12; ++k) {
            if (refr < 0.0001 || refr_shadow > 0.99)
                break;

            currentHit = traceRay(currentRo, currentRd);
            material = materials[currentHit.materialId];
            bool miss = currentHit.hit.t < EPSILON;
            if (miss) {
                refr_accum += texture(skybox, currentRd).xyz * refr;
            }

            if (miss)
                // refr_accum += texture(skybox, currentRd).xyz * refr;
                // refr_accum = vec3(0,1,0);
                break;

            pos = currentRo + currentHit.hit.t * currentRd;
            if (!currentHit.hit.inside) {
                refr_shadow += calculateShadow(pos) * (1 - material.transparency);
                n = refr_stack[stack_index] / material.n;
                stack_index++;
                refr_stack[stack_index] = material.n;
            } else {
                stack_index--;
                stack_index = max(stack_index, 0);
                n           = material.n / refr_stack[stack_index];
            }

            vec3 color = shade(currentHit, pos, -rd);
            refr_accum += color * refr;

            currentRd = refract(currentRd, currentHit.hit.normal, n); // lrd
            currentRo = pos + 0.0001 * -currentHit.hit.normal;
            refr *= material.transparency;

        }
        refr_shadow = clamp(refr_shadow, 0., 1.);
        accum += refr_accum * transmission_coef * (1 - refr_shadow) + refl_accum * reflection_coef * refl_shadow * dinv;
        // accum *= primaryShadow;

        fSpecDiff = vec4(accum, 1);
        // fSpecDiff.z = refr_shadow;
        // fSpecDiff = vec4(refl_shadow, 0, 0, 1);
        // fSpecDiff = vec4(refl_accum,1.);
        fAmbient = vec4(primaryColor * vec3(0.1), 1);

        // fSpecDiff = max(fSpecDiff, fAmbient);
        fShadow = primaryShadow;
        // fShadow =  primaryShadow;

    } else {
        fSpecDiff = vec4(texture(skybox, rd).rgb, 1.0);
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

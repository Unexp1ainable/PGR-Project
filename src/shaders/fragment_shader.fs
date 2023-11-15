#version 460
layout(location = 0) out vec4 fColor;

in vec4 gl_FragCoord;

#define ORIGIN  vec3(0, 0, 0)
#define EPSILON 0.00001

#define TYPE_SPHERE      0
#define TYPE_CYLINDER    1
#define TYPE_PLANE       2
#define TYPE_DISC        3
#define TYPE_POINT_LIGHT 4

struct Material {
    vec3 color; // diffuse color
    float n; // refraction index
    float roughness; // Cook-Torrance roughness
    float fresnel; // Cook-Torrance fresnel reflectance
    float density; // Cook-Torrance color density i.e. fraction of diffuse reflection
};

struct RenderItem {
    int type;
    vec3 position;
    vec3 normal;
    vec3 direction;
    float size;
    float radius;
    Material material;
};

struct Hit {
    float t;
    vec3 normal; // normal of the surface at hit point
    vec3 pos; // hit position
    bool inside;
};

#define NO_HIT Hit(-1, vec3(0), vec3(0), false)


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
uniform int shadowRays        = 4;
uniform vec3 lightPosition    = vec3(0, 4, 0);
uniform float n               = 1.2;
uniform uint time             = 42;

uniform float roughness = 0.68;
uniform float fresnel   = 0.5;
uniform float density   = 0.8;

Material materialRed   = Material(vec3(1, 0, 0), n, roughness, fresnel, 0.0);
Material materialGreen = Material(vec3(0, 1, 0), 1., roughness, fresnel, density);
Material materialBlue  = Material(vec3(0, 0, 1), 1., roughness, fresnel, density);
Material materialGray  = Material(vec3(0.2, 0.2, 0.2), 1., roughness, fresnel, density);
Material materialWhite = Material(vec3(1, 1, 1), 1., roughness, fresnel, density);


RenderItem createSphere(vec3 position, float radius, Material material)
{
    RenderItem item;
    item.type     = TYPE_SPHERE;
    item.position = position;
    item.radius   = radius;
    item.material = material;
    return item;
}

RenderItem createCylinder(vec3 position, vec3 direction, float radius, float height, Material material)
{
    RenderItem item;
    item.type      = TYPE_CYLINDER;
    item.position  = position;
    item.direction = direction;
    item.radius    = radius;
    item.size      = height;
    item.material  = material;
    return item;
}

RenderItem createPlane(vec3 position, vec3 normal, vec3 direction, float size, Material material)
{
    RenderItem item;
    item.type      = TYPE_PLANE;
    item.position  = position;
    item.normal    = normal;
    item.direction = direction;
    item.size      = size;
    item.material  = material;
    return item;
}

RenderItem createDisc(vec3 position, vec3 normal, vec3 direction, float size, Material material)
{
    RenderItem item;
    item.type      = TYPE_DISC;
    item.position  = position;
    item.normal    = normal;
    item.direction = direction;
    item.size      = size;
    item.material  = material;
    return item;
}

RenderItem createPointLight(vec3 position, Material material)
{
    RenderItem item;
    item.type     = TYPE_POINT_LIGHT;
    item.position = position;
    item.material = material;
    return item;
}

RenderItem sphere   = createSphere(vec3(1, 1, 2), 0.2, materialRed);
RenderItem cylinder = createCylinder(vec3(0, 0, 2), vec3(0, 1, 0), 0.2, 0.5, materialGreen);
RenderItem plane    = createPlane(vec3(-1, 0.5, 2), vec3(0, 1, 0), vec3(1, 0, 0), 0.5, materialBlue);
RenderItem light    = RenderItem(TYPE_POINT_LIGHT, lightPosition, vec3(0, 0, 0), vec3(0, 0, 0), 0, 2, materialWhite);

Hit intersectSphere(vec3 ro, vec3 rd, RenderItem sphere)
{
    vec3 sphereToRay = ro - sphere.position;

    float a = dot(rd, rd);
    float b = 2 * dot(rd, sphereToRay);
    float c = dot(sphere.position, sphere.position) + dot(ro, ro) - 2.0 * dot(sphere.position, ro) - sphere.radius * sphere.radius;
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
    return Hit(t, normalize(pos - sphere.position), pos, t == t2);
}

// Stolen from https://www.shadertoy.com/view/4lcSRn
Hit intersectCylinder(vec3 ro, vec3 rd, RenderItem cylinder)
{
    vec3 cb  = cylinder.position;
    vec3 ba  = cylinder.direction * cylinder.size; // oriented cylinder height
    vec3 pb  = cylinder.position + ba; // cylinder top
    float ra = cylinder.radius; // cylinder radius

    vec3 oc = ro - cb; // vector from cylinder base to ray origin

    float baba = dot(ba, ba); // square of cylinder height vector magnitude
    float bard = dot(ba, rd); // dot product between cylinder height vector and ray direction
    float baoc = dot(ba, oc); // dot product between cylinder height vector and vector from cylinder base to ray origin

    float k2 = baba - bard * bard;
    float k1 = baba * dot(oc, rd) - baoc * bard;
    float k0 = baba * dot(oc, oc) - baoc * baoc - ra * ra * baba;


    float h = k1 * k1 - k2 * k0;
    if (h < EPSILON)
        return NO_HIT;
    h        = sqrt(h);
    float t  = (-k1 - h) / k2;
    float t2 = (-k1 + h) / k2;

    // t = t < 0. ? t2 : t;

    // body
    float y = baoc + t * bard;
    if (y > EPSILON && y < baba && t > EPSILON)
        return Hit(t, ((oc + t * rd - ba * y / baba) / ra), ro + rd * t, true);

    y = baoc + t2 * bard;
    if (y > EPSILON && y < baba)
        return Hit(t2, -((oc + t2 * rd - ba * y / baba) / ra), ro + rd * t2, false);

    return NO_HIT;
}


// plane degined by p (p.xyz must be normalized)
Hit intersectPlane(vec3 ro, vec3 rd, RenderItem plane)
{
    vec3 o  = ro - plane.position;
    float t = -dot(plane.normal, o) / dot(rd, plane.normal);
    vec3 q  = o + rd * t;

    vec3 d1    = plane.direction;
    vec3 d2    = cross(plane.normal, d1);
    float dist = max(abs(dot(q, d1)), abs(dot(q, d2)));

    t = t > EPSILON && dist < plane.size ? t : -1;
    return Hit(t, plane.normal, ro + rd * t, false);
}

// plane degined by p (p.xyz must be normalized)
Hit intersectDisc(vec3 ro, vec3 rd, RenderItem plane)
{
    vec3 o  = ro - plane.position;
    float t = -dot(plane.normal, o) / dot(rd, plane.normal);
    vec3 q  = o + rd * t;

    float dist = length(q);

    t = t > EPSILON && dist < plane.size ? t : -1;
    return Hit(t, plane.normal, ro + rd * t, false);
}

Hit traceIntersect(vec3 ro, vec3 rd, RenderItem item)
{
    switch (item.type) {
        case TYPE_SPHERE:
            return intersectSphere(ro, rd, item);
        case TYPE_CYLINDER:
            return intersectCylinder(ro, rd, item);
        case TYPE_PLANE:
            return intersectPlane(ro, rd, item);
        case TYPE_DISC:
            return intersectDisc(ro, rd, item);
        case TYPE_POINT_LIGHT:
            return intersectSphere(ro, rd, item);
        default:
            return NO_HIT;
    }
}


HitInfo traceRay(vec3 ro, vec3 rd)
{
    const int itemCount         = 8;
    RenderItem floor_           = createPlane((vec3(0, 0, 0)), vec3(0, 1, 0), vec3(1, 0, 0), 5, materialGray);
    RenderItem right_           = createPlane((vec3(2.5, 0, 0)), vec3(1, 0, 0), vec3(1, 0, 0), 5, materialGray);
    RenderItem left_            = createPlane((vec3(-2.5, 0, 0)), vec3(1, 0, 0), vec3(1, 0, 0), 5, materialGray);
    RenderItem backside_        = createPlane((vec3(0, 0, 4)), vec3(0, 0, 1), vec3(1, 0, 0), 5, materialGray);
    RenderItem items[itemCount] = { sphere, cylinder, plane, light, floor_, right_, backside_, left_ };


    Hit result = NO_HIT;
    Material material;
    bool isLight = false;

    float min_t = 1000000;

    for (int i = 0; i < itemCount; ++i) {
        Hit hit = traceIntersect(ro, rd, items[i]);
        if (hit.t < EPSILON) {
            continue;
        }

        if (hit.t < min_t) {
            min_t    = hit.t;
            result   = hit;
            material = items[i].material;
            isLight  = items[i].type == TYPE_POINT_LIGHT;
        }
    }

    return HitInfo(result, -rd, material, isLight);
}

// https://www.shadertoy.com/view/XsXXDB
vec3 shadeBlinnPhong(HitInfo hitInfo, RenderItem light, float shadow)
{
    float diffuse  = 0.6;
    float specular = 0.4;
    vec3 n         = hitInfo.hit.normal;

    vec3 res = vec3(0.);
    vec3 ld  = normalize(light.position - hitInfo.hit.pos);
    res      = hitInfo.material.color * diffuse * dot(n, ld);
    vec3 h   = normalize(hitInfo.ed + ld);
    res += specular * pow(dot(n, h), 16.);

    // vec3 ambient = vec3(0.1, 0.1, 0.1) * hitInfo.material.color;
    // res          = max(res, ambient);
    // res          = shadowed ? ambient : res;
    return res;
}

// https://www.shadertoy.com/view/XsXXDB
vec3 shadeCookTorrance(HitInfo hitInfo, RenderItem light, float shadow)
{
    float roughness = hitInfo.material.roughness;
    float F0        = hitInfo.material.fresnel;
    float K         = hitInfo.material.density;
    //
    vec3 ld     = normalize(light.position - hitInfo.hit.pos);
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
    float fres = pow(1.0 - VdotH, 5.);
    fres *= (1.0 - F0);
    fres += F0;

    vec3 spec = (NdotV * NdotL == 0.) ? vec3(0.) : vec3(fres * geo * rough) / (NdotV * NdotL);
    vec3 res  = NdotL * ((1. - K) * spec + K * hitInfo.material.color) * light.material.color;
    res *= shadow;
    res = max(res, 0.1 * hitInfo.material.color);

    return res;
}

vec3 shade(HitInfo hitInfo, RenderItem light, float shadow)
{
    // return shadeBlinnPhong(hitInfo, light, shadowed);
    return shadeCookTorrance(hitInfo, light, shadow);
}

float random(vec2 st)
{
    // st *= time;
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}


vec2 randomSampleUnitCircle(vec2 st)
{
    float seed  = random(st);
    float r     = sqrt(seed);
    float theta = 6.28318530718 * seed;
    return vec2(r, theta);
}


float calculateShadow(vec3 pos, RenderItem light)
{
    vec3 lightDir      = normalize(light.position - pos);
    vec3 perpLightDir1 = normalize(cross(lightDir, vec3(lightDir.x + 1, lightDir.y + 1, 0)));
    vec3 perpLightDir2 = normalize(cross(lightDir, perpLightDir1));
    vec2 st = gl_FragCoord.xy / vec2(screenWidth, screenHeight);

    // HitInfo shadowHit = traceRay(pos + 0.001 * lightDir, lightDir);
    // vec3 shadowHitPos = pos + shadowHit.hit.t * lightDir;
    // bool res          = shadowHit.hit.t > EPSILON && shadowHit.hit.t < (length(light.position - shadowHitPos) - light.radius);
    // return 1. - float(res);

    float shadow = 0;
    int ashadowRays = 32; // TODO remove


    for (int i = 0; i < ashadowRays; i++) {
        vec2 rsample = randomSampleUnitCircle(st);
        // rsample      = vec2(0);
        // vec3 offset = vec3(rsample.x * cos(rsample.y), rsample.x * sin(rsample.y), 0);
        float r = rsample.x * light.radius;
        float theta = rsample.y;
        vec2 cartesian = vec2(r * cos(theta), r * sin(theta));

        vec3 offset = perpLightDir1 * cartesian.x + perpLightDir2 * cartesian.y;
        vec3 rayDir = normalize(light.position + offset - pos);

        HitInfo shadowHit = traceRay(pos + 0.001 * lightDir, rayDir);
        vec3 shadowHitPos = pos + shadowHit.hit.t * rayDir;
        bool res          = shadowHit.hit.t > EPSILON && shadowHit.hit.t < (length(light.position - shadowHitPos) - light.radius);

        shadow += float(!res);
        st += rsample.xx;
    }
    return shadow / float(ashadowRays);
}

float calculateShadowHard(vec3 pos, RenderItem light)
{
    vec3 lightDir      = normalize(light.position - pos);

    HitInfo shadowHit = traceRay(pos + 0.001 * lightDir, lightDir);
    vec3 shadowHitPos = pos + shadowHit.hit.t * lightDir;
    bool res          = shadowHit.hit.t > EPSILON && shadowHit.hit.t < (length(light.position - shadowHitPos) - light.radius);
    return 1. - float(res);
}

vec4 whatColorIsThere(vec3 ro, vec3 rd)
{
    HitInfo hitInfo = traceRay(ro, rd);
    Hit hit         = hitInfo.hit;

    if (hit.t > EPSILON) {
        if (hitInfo.isLight) {
            return vec4(hitInfo.material.color, 1);
        }

        vec3 pos = hit.pos;

        float shadow = calculateShadow(pos, light);
        vec3 color   = shade(hitInfo, light, shadow);

        HitInfo currentHit = hitInfo;
        vec3 currentRo     = pos;
        vec3 currentRd     = reflect(rd, currentHit.hit.normal);
        currentRo += 0.0001 * currentRd;
        float refl = 1;
        vec3 accum = color;

        // handle reflections
        for (int k = 1; k < reflectionBounces; ++k) {
            refl *= 1. - hitInfo.material.density;

            currentHit = traceRay(currentRo, currentRd);
            shadow     = calculateShadowHard(currentHit.hit.pos, light);

            vec3 color = shade(currentHit, light, shadow);

            accum += color * refl;
            if ((currentHit.material.density < .0))
                break;

            currentRo = currentHit.hit.pos;
            currentRd = reflect(currentRd, currentHit.hit.normal);
            currentRo += 0.0001 * currentRd;
        }

        // handle refraction
        // if (hitInfo.material.transparency < 1.) {
        currentHit = hitInfo; // li
        currentRo  = pos; // lro
        currentRd  = reflect(rd, currentHit.hit.normal); // lrd
        currentRo += 0.0001 * currentRd;
        float refr = 1;


        float n     = 1. / currentHit.material.n;
        float cosI  = -dot(currentHit.hit.normal, currentHit.ed);
        float cost2 = 1. - n * n * (1. - cosI * cosI);
        if (cost2 > 0.) {
            currentRo = currentHit.hit.pos;
            currentRd = normalize(-currentHit.ed * n + currentHit.hit.normal * (n * cosI - sqrt(cost2)));
            currentRo += 0.0001 * currentRd;
            for (int k = 1; k < 4; ++k) {
                // li.d = -1.;
                refr *= 1. - currentHit.material.density;
                //
                currentHit = traceRay(currentRo, currentRd);
                shadow     = calculateShadowHard(currentHit.hit.pos, light);
                vec3 color = shade(currentHit, light, shadow);
                //
                accum += color * refr;
                // float d = length(currentHit.hit.pos - currentRo);
                // if ((li.d<.0)||(!li.mat.refraction)) break;
                if (currentHit.hit.inside)
                    n = currentHit.material.n;
                else
                    n = 1. / currentHit.material.n;
                cosI  = -dot(currentHit.hit.normal, currentHit.ed);
                cost2 = 1. - n * n * (1. - cosI * cosI);
                if (cost2 <= 0.)
                    break;
                currentRo = currentHit.hit.pos;
                currentRd = normalize(-currentHit.ed * n + currentHit.hit.normal * (n * cosI - sqrt(cost2)));
                currentRo += 0.0001 * currentRd;
                // }
            }
        }


        return vec4(accum, 1);

    } else {
        return vec4(0, 0, 0, 1);
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

    fColor = whatColorIsThere(ro, rd);
}

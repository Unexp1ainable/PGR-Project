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

struct RenderItem {
    int type;
    vec3 position;
    vec3 normal;
    vec3 direction;
    float size;
    float radius;
    vec3 color;
};

#define NO_HIT -1

struct Hit {
    float t;
    vec3 normal;
};

struct HitInfo {
    Hit hit;
    vec3 color;
    bool isLight;
};

uniform int screenWidth    = 1024;
uniform int screenHeight   = 768;
uniform mat4 cameraMatrix  = mat4(1.0);


RenderItem createSphere(vec3 position, float radius, vec3 color)
{
    RenderItem item;
    item.type     = TYPE_SPHERE;
    item.position = position;
    item.radius   = radius;
    item.color    = color;
    return item;
}

RenderItem createCylinder(vec3 position, vec3 direction, float radius, float height, vec3 color)
{
    RenderItem item;
    item.type      = TYPE_CYLINDER;
    item.position  = position;
    item.direction = direction;
    item.radius    = radius;
    item.size      = height;
    item.color    = color;
    return item;
}

RenderItem createPlane(vec3 position, vec3 normal, vec3 direction, float size, vec3 color)
{
    RenderItem item;
    item.type      = TYPE_PLANE;
    item.position  = position;
    item.normal    = normal;
    item.direction = direction;
    item.size      = size;
    item.color    = color;
    return item;
}

RenderItem createDisc(vec3 position, vec3 normal, vec3 direction, float size, vec3 color)
{
    RenderItem item;
    item.type      = TYPE_DISC;
    item.position  = position;
    item.normal    = normal;
    item.direction = direction;
    item.size      = size;
    item.color    = color;
    return item;
}

RenderItem createPointLight(vec3 position, vec3 color)
{
    RenderItem item;
    item.type     = TYPE_POINT_LIGHT;
    item.position = position;
    item.color    = color;
    return item;
}

RenderItem sphere   = createSphere(vec3(1, 1, 2), 0.2, vec3(1, 0, 0));
RenderItem cylinder = createCylinder(vec3(0, 0, 2), vec3(0, 1, 0), 0.2, 0.5, vec3(0, 0, 1));
RenderItem plane    = createPlane(vec3(-1, 0.5, 2), vec3(0, 1, 0), vec3(1, 0, 0), 0.5, vec3(0, 1, 0));
uniform RenderItem light = RenderItem(
    TYPE_POINT_LIGHT, vec3(100, 100, -100), vec3(0, 0, 0), vec3(0, 0, 0), 0, 2, vec3(1, 1, 1)
);

Hit intersectSphere(vec3 ro, vec3 rd, RenderItem sphere)
{
    vec3 sphereToRay = ro - sphere.position;

    float a = dot(rd, rd);
    float b = 2 * dot(rd, sphereToRay);
    float c = dot(sphere.position, sphere.position) + dot(ro, ro) - 2.0 * dot(sphere.position, ro)
        - sphere.radius * sphere.radius;
    float d = b * b - 4 * a * c;
    if (d <= EPSILON) {
        return Hit(NO_HIT, vec3(0, 0, 0));
    }
    float t1 = (-b - sqrt(d)) / (2 * a);
    float t2 = (-b + sqrt(d)) / (2 * a);

    float t  = t1 > EPSILON * 10 ? t1 : t2;
    t        = t > EPSILON * 10 ? t : NO_HIT;
    vec3 pos = ro + rd * t;
    return Hit(t, normalize(pos - sphere.position));
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
    float baoc = dot(
        ba, oc
    ); // dot product between cylinder height vector and vector from cylinder base to ray origin

    float k2 = baba - bard * bard;
    float k1 = baba * dot(oc, rd) - baoc * bard;
    float k0 = baba * dot(oc, oc) - baoc * baoc - ra * ra * baba;


    float h = k1 * k1 - k2 * k0;
    if (h < 0.0)
        return Hit(NO_HIT, vec3(0, 0, 0));
    h        = sqrt(h);
    float t  = (-k1 - h) / k2;
    float t2 = (-k1 + h) / k2;

    // t = t < 0. ? t2 : t;

    // body
    float y = baoc + t * bard;
    if (y > EPSILON && y < baba && t > EPSILON)
        return Hit(t, ((oc + t * rd - ba * y / baba) / ra));

    t = t2;
    y = baoc + t * bard;
    if (y > EPSILON && y < baba)
        return Hit(t, -((oc + t * rd - ba * y / baba) / ra));

    return Hit(NO_HIT, vec3(0, 0, 0));
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

    t = t > EPSILON && dist < plane.size ? t : NO_HIT;
    return Hit(t, plane.normal);
}

// plane degined by p (p.xyz must be normalized)
Hit intersectDisc(vec3 ro, vec3 rd, RenderItem plane)
{
    vec3 o  = ro - plane.position;
    float t = -dot(plane.normal, o) / dot(rd, plane.normal);
    vec3 q  = o + rd * t;

    float dist = length(q);

    t = t > EPSILON && dist < plane.size ? t : NO_HIT;
    return Hit(t, plane.normal);
}

Hit traceIntersect(vec3 ro, vec3 rd, RenderItem item) {
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
        return Hit(NO_HIT, vec3(0, 0, 0));
    }
}


HitInfo traceRay(vec3 ro, vec3 rd)
{
    const int itemCount = 5;
    RenderItem floor_ = createPlane((vec3(0, 0, 0)), vec3(0, 1, 0), vec3(1, 0, 0), 5, vec3(0.3, 0.3, 0.3));
    RenderItem items[itemCount] = { sphere, cylinder, plane, light, floor_ };


    // const int COUNT = 4;
    // Hit hits[COUNT];
    // hits[0] = intersectSphere(ro, rd, sphere);
    // hits[1] = intersectCylinder(ro, rd, cylinder);
    // hits[2] = intersectDisc(ro, rd, plane);


    Hit result  = Hit(NO_HIT, vec3(0, 0, 0));
    vec3 color = vec3(0, 0, 0);
    bool isLight = false;

    float min_t = 1000000;
    for (int i = 0; i < itemCount; i++) {
        Hit hit = traceIntersect(ro, rd, items[i]);
        if (hit.t < EPSILON) {
            continue;
        }

        if (hit.t < min_t) {
            min_t  = hit.t;
            result = hit;
            color = items[i].color;
            isLight = items[i].type == TYPE_POINT_LIGHT;
        }
    }

    return HitInfo(result, color, isLight);
}

// https://www.shadertoy.com/view/XsXXDB
vec3 shadeBlinnPhong( HitInfo hitInfo, vec3 pos, vec3 rd, vec3 lp, bool shadowed)
{
	float diffuse = 0.6;
	float specular = 0.4;
    vec3 n = hitInfo.hit.normal;

	vec3 res = vec3(0.);
	vec3 ld = normalize(lp-pos);
	res = hitInfo.color*diffuse*dot(n,ld);
	vec3 h = normalize(rd+ld);
	res += specular*pow(dot(n,h), 16.);

    vec3 ambient = vec3(0.1, 0.1, 0.1) * hitInfo.color;
    res = max(res, ambient);
    res = shadowed ? ambient : res;
	return res;
}

void main()
{
    float bigger  = screenWidth > screenHeight ? screenWidth : screenHeight;
    float waspect = screenWidth / bigger;
    float haspect = screenHeight / bigger;

    vec3 ro = (cameraMatrix * vec4(ORIGIN, 1)).xyz;
    vec3 re
        = (cameraMatrix
           * vec4(
               vec3(gl_FragCoord.x / bigger - 0.5 * waspect, gl_FragCoord.y / bigger - 0.5 * haspect, 1), 1
           ))
              .xyz;

    vec3 rd = normalize(re - ro);

    HitInfo hitInfo = traceRay(ro, rd);
    Hit hit         = hitInfo.hit;

    if (hit.t > EPSILON) {
        if (hitInfo.isLight) {
            fColor = vec4(hitInfo.color, 1);
            return;
        }

        vec3 pos = ro + hit.t * rd;
        // shadow ray
        HitInfo shadowHit     = traceRay(pos, normalize(light.position - pos));
        vec3 shadowHitPos = pos + shadowHit.hit.t * normalize(light.position - pos);
        bool shadowed = shadowHit.hit.t > EPSILON && shadowHit.hit.t < (length(light.position - shadowHitPos) - light.radius);
        vec3 ambient = vec3(0.1, 0.1, 0.1);

        vec3 colorBP = shadeBlinnPhong(hitInfo, pos, -rd, light.position, shadowed);

        fColor = vec4(colorBP, 1);
    } else {
        fColor = vec4(0, 0, 0, 1);
    }
}

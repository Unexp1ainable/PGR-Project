#version 460
layout(location = 0) out vec4 fColor;

in vec4 gl_FragCoord;

#define ORIGIN vec3(0, 0, 0)

struct Sphere {
    vec3 center;
    float radius;
};

struct Cylinder {
    vec3 center;
    vec3 direction;
    float radius;
    float height;
};

struct Plane {
    vec3 center;
    vec3 normal;
    vec3 direction;
    float size;
};

#define NO_HIT -1

struct Hit {
    float t;
    vec3 normal;
};


uniform vec3 lightPosition = vec3(100, 100, -100);
uniform int screenWidth    = 1024;
uniform int screenHeight   = 768;
uniform mat4 cameraMatrix  = mat4(1.0);


Sphere sphere     = Sphere(vec3(1, 0, 2), 0.2);
Cylinder cylinder = Cylinder(vec3(0, 0, 2), vec3(0, 1, 0), 0.2, 0.5);
Plane plane       = Plane(vec3(-1, 0, 2), vec3(0, 1, 0), vec3(1, 0, 0), .5);


Hit intersectSphere(vec3 ro, vec3 rd, Sphere sphere)
{
    vec3 sphereToRay = ro - sphere.center;

    float a = dot(rd, rd);
    float b = 2 * dot(rd, sphereToRay);
    float c = dot(sphere.center, sphere.center) + dot(ro, ro) - 2.0 * dot(sphere.center, ro)
        - sphere.radius * sphere.radius;
    float d = b * b - 4 * a * c;
    if (d <= 0.) {
        return Hit(NO_HIT, vec3(0, 0, 0));
    }
    float t1 = (-b - sqrt(d)) / (2 * a);
    float t2 = (-b + sqrt(d)) / (2 * a);

    float t  = t1 < 0. ? t2 : t1;
    vec3 pos = ro + rd * t;
    return Hit(t, normalize(pos - sphere.center));
}

// Stolen from https://www.shadertoy.com/view/4lcSRn
Hit intersectCylinder(vec3 ro, vec3 rd, Cylinder cylinder)
{
    vec3 cb  = cylinder.center;
    vec3 ba  = cylinder.direction * cylinder.height; // oriented cylinder height
    vec3 pb  = cylinder.center + ba; // cylinder top
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
    h       = sqrt(h);
    float t = (-k1 - h) / k2;

    // body
    float y = baoc + t * bard;
    if (y > 0.0 && y < baba)
        return Hit(t, (oc + t * rd - ba * y / baba) / ra);

    // caps
    t = (((y < 0.0) ? 0.0 : baba) - baoc) / bard;
    if (abs(k1 + k2 * t) < h)
        return Hit(t, ba * sign(y) / sqrt(baba));

    return Hit(NO_HIT, vec3(0, 0, 0));
}


// plane degined by p (p.xyz must be normalized)
Hit intersectPlane(vec3 ro, vec3 rd, Plane plane)
{
    vec3 o  = ro - plane.center;
    float t = -dot(plane.normal, o) / dot(rd, plane.normal);
    vec3 q  = o + rd * t;

    vec3 d1    = plane.direction;
    vec3 d2    = cross(plane.normal, d1);
    float dist = max(abs(dot(q, d1)), abs(dot(q, d2)));

    // for disc:
    // float mdist = sqrt(dot(dist,dist));
    // bool res = t > 0. && dot(q,q)<1*1;

    t = t > 0. && dist < plane.size ? t : NO_HIT;
    return Hit(t, plane.direction);
}

Hit traceRay(vec3 ro, vec3 rd)
{
    const int COUNT = 3;
    Hit hits[COUNT];
    hits[0] = intersectSphere(ro, rd, sphere);
    hits[1] = intersectCylinder(ro, rd, cylinder);
    hits[2] = intersectPlane(ro, rd, plane);

    Hit result  = Hit(NO_HIT, vec3(0, 0, 0));
    float min_t = 1000000;
    for (int i = 0; i < COUNT; i++) {
        if (hits[i].t < 0.) {
            continue;
        }

        if (hits[i].t < min_t) {
            min_t  = hits[i].t;
            result = hits[i];
        }
    }

    return result;
}


void main()
{
    float bigger  = screenWidth > screenHeight ? screenWidth : screenHeight;
    float waspect = screenWidth / bigger;
    float haspect = screenHeight / bigger;

    vec3 ro = (cameraMatrix * vec4(ORIGIN, 1)).xyz;
    vec3 re = (cameraMatrix
        * vec4(vec3(gl_FragCoord.x / bigger - 0.5 * waspect, gl_FragCoord.y / bigger - 0.5 * haspect, 1), 1)).xyz;

    vec3 rd = normalize(re - ro);

    Hit hit = traceRay(ro, rd);

    // Hit hit = traceRay(
    //     ORIGIN, vec3(gl_FragCoord.x / bigger - 0.5 * waspect, gl_FragCoord.y / bigger - 0.5 * haspect, 1)
    // );
    if (hit.t > 0.) {
        vec3 normal        = hit.normal;
        vec3 surface2light = normalize(lightPosition - ro + hit.t * rd);
        float diffuse      = max(dot(normal, surface2light), 0.1f);
        fColor             = vec4(diffuse, 0, 0, 1);
    } else {
        fColor = vec4(0, 0, 0, 1);
    }
}

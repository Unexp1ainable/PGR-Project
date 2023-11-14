#version 460
layout(location = 0) out vec4 fColor;

in vec4 gl_FragCoord;

#define ORIGIN vec3(0, 0, 0)
#define EPSILON 0.00001


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
Plane plane       = Plane(vec3(-1, 0.5, 2), vec3(0, 1, 0), vec3(1, 0, 0), .5);


Hit intersectSphere(vec3 ro, vec3 rd, Sphere sphere)
{
    vec3 sphereToRay = ro - sphere.center;

    float a = dot(rd, rd);
    float b = 2 * dot(rd, sphereToRay);
    float c = dot(sphere.center, sphere.center) + dot(ro, ro) - 2.0 * dot(sphere.center, ro)
        - sphere.radius * sphere.radius;
    float d = b * b - 4 * a * c;
    if (d <= EPSILON) {
        return Hit(NO_HIT, vec3(0, 0, 0));
    }
    float t1 = (-b - sqrt(d)) / (2 * a);
    float t2 = (-b + sqrt(d)) / (2 * a);

    float t  = t1 > EPSILON*10 ? t1 : t2;
    t = t > EPSILON*10 ? t : NO_HIT;
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
    // bool res = t > EPSILON && dot(q,q)<1*1;

    t = t > EPSILON && dist < plane.size ? t : NO_HIT;
    return Hit(t, plane.normal);
}

// plane degined by p (p.xyz must be normalized)
Hit intersectDisc(vec3 ro, vec3 rd, Plane plane)
{
    vec3 o  = ro - plane.center;
    float t = -dot(plane.normal, o) / dot(rd, plane.normal);
    vec3 q  = o + rd * t;

    float dist = length(q);

    // for disc:
    // float mdist = sqrt(dot(dist,dist));
    // bool res = t > EPSILON && dot(q,q)<1*1;

    t = t > EPSILON && dist < plane.size ? t : NO_HIT;
    return Hit(t, plane.normal);
}

Hit traceRay(vec3 ro, vec3 rd)
{
    const int COUNT = 4;
    Hit hits[COUNT];
    hits[0] = intersectSphere(ro, rd, sphere);
    hits[1] = intersectCylinder(ro, rd, cylinder);
    hits[2] = intersectDisc(ro, rd, plane);

    // box
    hits[3] = intersectPlane(ro, rd, Plane((vec3(0, 0, 0)), vec3(0, 1, 0), vec3(1, 0, 0), 5));
    // hits[4] = intersectPlane(ro, rd, Plane((vec3(2.5, 2.5, 0)), vec3(-1, 0, 0), vec3(0, 1, 0), 5));
    // hits[5] = intersectPlane(ro, rd, Plane((vec3(0, 2.5, 2.5)), vec3(0, 0, -1), vec3(0, 1, 0), 5));

    Hit result  = Hit(NO_HIT, vec3(0, 0, 0));
    float min_t = 1000000;
    for (int i = 0; i < COUNT; i++) {
        if (hits[i].t < EPSILON) {
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
    vec3 re
        = (cameraMatrix
           * vec4(
               vec3(gl_FragCoord.x / bigger - 0.5 * waspect, gl_FragCoord.y / bigger - 0.5 * haspect, 1), 1
           ))
              .xyz;

    vec3 rd = normalize(re - ro);

    Hit hit = traceRay(ro, rd);

    if (hit.t > EPSILON) {
        vec3 pos = ro + hit.t * rd;
        // shadow ray
        Hit shadowHit = traceRay(pos, normalize(lightPosition - pos));
        int notHardShadow = int(shadowHit.t < EPSILON || shadowHit.t > length(lightPosition - pos));

        vec3 normal        = hit.normal;
        vec3 surface2light = normalize(lightPosition - pos);
        // float diffuse      = max(dot(normal, surface2light), 0.1f);

        float s    = 10;
        vec3 L     = surface2light;
        vec3 N     = hit.normal;
        vec3 V     = rd;
        vec3 R     = reflect(V, N);
        float refl = 0.5;

        float dF = max(dot(L, N), 0);

        float sF = pow(max(dot(R, L), 0), s);
        vec3 color = vec3(1.,0.,0.);
        vec3 lightColor = vec3(1.,1.,1.);

        vec3 diffuse  = max(color * lightColor * dF * notHardShadow, vec3(0.1,0,0));
        vec3 specular = lightColor * sF * refl * notHardShadow;

        vec3 phong = diffuse + specular;

        fColor = vec4( phong, 1);
    } else {
        fColor = vec4(0, 0, 0, 1);
    }
}

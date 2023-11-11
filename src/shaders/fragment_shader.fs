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

struct Hit {
    bool hit;
    vec3 pos;
    vec3 normal;
};


uniform vec3 lightPosition = vec3(100, 100, -100);
uniform Sphere sphere      = Sphere(vec3(0, 0, 2), 0.2);
uniform Cylinder cylinder  = Cylinder(vec3(0, 0, 2), vec3(0, 0, 2), 0.2, 0.5);
uniform int screenWidth    = 1024;
uniform int screenHeight   = 768;


Hit intersectSphere(vec3 from, vec3 to, Sphere sphere)
{
    vec3 ray         = to - from;
    vec3 sphereToRay = from - sphere.center;

    float a = dot(ray, ray);
    float b = 2 * dot(ray, sphereToRay);
    float c = dot(sphere.center, sphere.center) + dot(from, from) - 2.0 * dot(sphere.center, from)
        - sphere.radius * sphere.radius;
    float d = b * b - 4 * a * c;
    if (d <= 0.) {
        return Hit(false, vec3(0, 0, 0), vec3(0, 0, 0));
    }
    float t1 = (-b - sqrt(d)) / (2 * a);
    float t2 = (-b + sqrt(d)) / (2 * a);

    float t  = t1 < 0. ? t2 : t1;
    vec3 pos = ray * t;
    bool res = t > 0.;
    return Hit(res, pos, normalize(pos - sphere.center));
}

// Stolen from https://www.shadertoy.com/view/4lcSRn
Hit intersectCylinder(vec3 from, vec3 to, Cylinder cylinder)
{
    vec3 ray    = to - from;
    vec3 center = cylinder.center;

    vec3 ro  = from;
    vec3 rd  = normalize(ray);
    vec3 pa  = center;
    vec3 pb  = center + cylinder.direction * cylinder.height;
    float ra = cylinder.radius;

    vec3 ba = pb - pa;
    vec3 oc = ro - pa;

    float baba = dot(ba, ba);
    float bard = dot(ba, rd);
    float baoc = dot(ba, oc);

    float k2 = baba - bard * bard;
    float k1 = baba * dot(oc, rd) - baoc * bard;
    float k0 = baba * dot(oc, oc) - baoc * baoc - ra * ra * baba;


    float h = k1 * k1 - k2 * k0;
    if (h < 0.0)
        return Hit(false, vec3(0, 0, 0), vec3(0, 0, 0));
    h       = sqrt(h);
    float t = (-k1 - h) / k2;

    // body
    float y = baoc + t * bard;
    if (y > 0.0 && y < baba)
        return Hit(t.x > 0., t * from, (oc + t * rd - ba * y / baba) / ra);

    // caps
    t = ( ((y<0.0) ? 0.0 : baba) - baoc)/bard;
    if( abs(k1+k2*t)<h ) return Hit( true, t * from, ba*sign(y)/sqrt(baba) );

    return Hit(false, vec3(0, 0, 0), vec3(0, 0, 0));
}


Hit traceRay(vec3 from, vec3 to)
{
    // return intersectSphere(from, to, sphere);
    return intersectCylinder(from, to, cylinder);
}


void main()
{
    float bigger  = screenWidth > screenHeight ? screenWidth : screenHeight;
    float waspect = screenWidth / bigger;
    float haspect = screenHeight / bigger;

    Hit hit = traceRay(
        ORIGIN, vec3(gl_FragCoord.x / bigger - 0.5 * waspect, gl_FragCoord.y / bigger - 0.5 * haspect, 1)
    );
    if (hit.hit) {
        vec3 normal        = hit.normal;
        vec3 surface2light = normalize(lightPosition - hit.pos);
        float diffuse      = max(dot(normal, surface2light), 0.1f);
        fColor             = vec4(diffuse, 0, 0, 1);
    } else {
        fColor = vec4(0, 0, 0, 1);
    }
}

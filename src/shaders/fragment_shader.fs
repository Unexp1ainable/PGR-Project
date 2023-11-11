#version 460
layout(location = 0) out vec4 fColor;

in vec4 gl_FragCoord;

#define ORIGIN vec3(0, 0, 0)

struct Sphere
{
    vec3 center;
    float radius;
};

struct Cylinder
{
    vec3 center;
    float radius;
    float height;
};

struct Hit {
    bool hit;
    vec3 pos;
};


uniform vec3 lightPosition = vec3(100,100,-100);
uniform Sphere sphere = Sphere(vec3(0, 0, 2), 0.2);
uniform int screenWidth = 1024;
uniform int screenHeight = 768;


Hit traceRay(vec3 from, vec3 to) {
    vec3 ray = to - from;
    vec3 sphereToRay = sphere.center - from;

    float a = dot(ray, ray);
    float b = 2 * dot(ray, sphereToRay);
    float c = dot(sphere.center, sphere.center) + dot(from,from) - 2.0 * dot(sphere.center, from) - sphere.radius * sphere.radius;
    float d = b * b - 4 * a * c;
    if (d <= 0.) {
        return Hit(false, vec3(0, 0, 0));
    }
    float t1 = (b - sqrt(d)) / (2 * a);
    float t2 = (b + sqrt(d)) / (2 * a);

    float t = t1 < 0. ? t2 : t1;
    vec3 pos = ray * t;
    bool res = t > 0.;
    return Hit(res, pos);
}


void main() 
{   
    float bigger = screenWidth > screenHeight ? screenWidth : screenHeight;
    float waspect = screenWidth / bigger;
    float haspect = screenHeight / bigger;

    Hit hit = traceRay(ORIGIN, vec3(gl_FragCoord.x/bigger-0.5*waspect, gl_FragCoord.y/bigger-0.5*haspect, 1));
    if (hit.hit) {
        vec3 normal = normalize(hit.pos - sphere.center);
        vec3 surface2light = normalize(lightPosition - hit.pos);
        float diffuse = max(dot(normal,surface2light),0.1f);
        fColor = vec4(1.*diffuse, 0, 0, 1);
    } else {
        fColor = vec4(0, 0, 0, 1);
    }
}

#version 460
layout(location = 0) out vec4 fColor;

in vec4 gl_FragCoord;
struct Sphere
{
    vec3 center;
    float radius;
};

struct Hit {
    bool hit;
    vec3 pos;
};

uniform vec3 lightPosition = vec3(100,100,-100);
Sphere test = Sphere(vec3(0, 0, 2), 0.2);

Hit traceRay(vec3 ray) {
    float a = dot(ray, ray);
    float b = 2 * dot(ray, test.center);
    float c = dot(test.center, test.center) - test.radius * test.radius;
    float d = b * b - 4 * a * c;
    if (d < 0) {
        return Hit(false, vec3(0, 0, 0));
    }
    float t = (-b - sqrt(d)) / (2 * a);
    return Hit(true, ray * -t);
}

void main() 
{ 
    int width = 1024;
    int height = 768;

    float bigger = width > height ? width : height;
    float waspect = width / bigger;
    float haspect = height / bigger;

    Hit hit = traceRay(vec3(gl_FragCoord.x/bigger-0.5*waspect, gl_FragCoord.y/bigger-0.5*haspect, 1));
    if (hit.hit) {
        vec3 normal = normalize(test.center - hit.pos);
        vec3 surface2light = normalize(lightPosition - hit.pos);
        float diffuse = max(dot(normal,surface2light),0.1f);
        fColor = vec4(1.*diffuse, 0, 0, 1);
    } else {
        fColor = vec4(0, 0, 0, 1);
    }
}

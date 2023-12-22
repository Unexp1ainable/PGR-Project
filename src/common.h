#pragma once
#include <geGL/Generated/OpenGLTypes.h>
#include <glm/glm.hpp>


class UniformStore {
public:
    int screenWidth          = 1024;
    int screenHeight         = 746;
    glm::mat4 cameraMatrix   = glm::mat4(1.0f);
    glm::vec3 lightPosition  = { 100, 100, -100 };
    float roughness = 0.68;
    float transparency   = 0.5;
    float density   = 0.8;
    float n = 1.2;
    glm::uint32 time;
};


class UniformSynchronizer {
public:
    UniformSynchronizer(GLuint program)
        : m_program(program)
    {
    }

    void syncUniforms(UniformStore const& store);

private:
    UniformStore m_gpu_uniforms {};
    GLuint m_program;
};


struct Sphere {
    glm::vec3 center;
    float radius;
};


struct Cylinder {
    glm::vec3 center;
    float radius;
    float height;
};

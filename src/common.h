#pragma once
#include <geGL/Generated/OpenGLTypes.h>
#include <glm/glm.hpp>


class UniformStore {
public:
    int screenWidth             = 1024;
    int screenHeight            = 746;
    glm::vec3 lightPosition     = { 100, 100, -100 };
    glm::vec3 spherePosition    = { 0., 0., 2. };
    glm::vec3 cylinderPosition  = { 0., 0., 2. };
    glm::vec3 cylinderDirection = { 0., 1., 0. };
};


class UniformSynchronizer {
public:
    UniformSynchronizer(GLuint program)
        : m_program(program)
    {
    }

    void syncUniforms(UniformStore const& store, glm::mat4 view);

private:
    UniformStore m_gpu_uniforms {};
    GLuint m_program;
    glm::mat4 m_view;
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

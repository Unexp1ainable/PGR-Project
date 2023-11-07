#pragma once
#include <geGL/Generated/OpenGLTypes.h>
#include <glm/glm.hpp>


class UniformStore {
public:
    int screenWidth         = 1024;
    int screenHeight        = 746;
    glm::vec3 lightPosition = { 0., 0., 0. };
};


class UniformSynchronizer {
public:
    UniformSynchronizer(GLuint program) : m_program(program) {}

    void syncUniforms(UniformStore const& store);

private:
    UniformStore m_gpu_uniforms{};
    GLuint m_program;
};

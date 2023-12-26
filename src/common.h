#pragma once
#include "constants.h"
#include <geGL/Generated/OpenGLTypes.h>
#include <glm/fwd.hpp>
#include <glm/glm.hpp>
#include "chessboard.h"

class UniformStore {
public:
    int screenWidth         = DEFAULT_WINDOW_WIDTH;
    int screenHeight        = DEFAULT_WINDOW_HEIGHT;
    glm::mat4 cameraMatrix  = glm::mat4(1.0f);
    glm::vec3 lightPosition = { 5, 8, 5 };
    float roughness         = 0.68;
    float transparency      = 0.5;
    float density           = 0.8;
    float n                 = 1.5;
    chessboard::Configuration chessBoard{};
    glm::uint32 time;
};


class UniformSynchronizer {
public:
    UniformSynchronizer(GLuint program)
        : m_program(program)
    {
    }

    void syncUniforms(UniformStore const& store);
    void syncUniformsForce(UniformStore const& store);

private:
    UniformStore m_gpu_uniforms {};
    GLuint m_program;
};

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
    glm::uint32_t accumCounter      = 1;
    chessboard::Configuration chessBoard{};
    glm::uint32 time;
};


class UniformSynchronizer {
public:
    UniformSynchronizer(GLuint firstPassProgram, GLuint secondPassProgram)
        : m_program1(firstPassProgram), m_program2(secondPassProgram)
    {
    }

    void syncUniforms(UniformStore const& store);
    void syncUniformsForce(UniformStore const& store);

private:
    UniformStore m_gpu_uniforms {};
    GLuint m_program1;
    GLuint m_program2;
};

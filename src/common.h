#pragma once
#include "chessboard.h"
#include "constants.h"
#include <geGL/Generated/OpenGLTypes.h>
#include <glm/fwd.hpp>
#include <glm/glm.hpp>

class UniformStore {
public:
    int screenWidth         = DEFAULT_WINDOW_WIDTH;
    int screenHeight        = DEFAULT_WINDOW_HEIGHT;
    glm::mat4 cameraMatrix  = glm::mat4(1.0f);
    glm::vec3 lightPosition = { 5, 8, 5 };
    int reflectionBounces   = 3;
    int refractionBounces   = 8;
    int shadowRays          = 1;

    float blackN            = 1.2;
    float blackRoughness    = 0.68;
    float blackTransparency = 0.5;
    float blackDensity      = 0.8;
    glm::vec3 blackColor    = glm::vec3(0.2, 0.2, 0.2);

    float whiteN            = 1.2;
    float whiteRoughness    = 0.68;
    float whiteTransparency = 0.5;
    float whiteDensity      = 0.8;
    glm::vec3 whiteColor    = glm::vec3(0.8, 0.8, 0.8);

    chessboard::Configuration chessBoard {};
    glm::uint32 time;
};


class UniformSynchronizer {
public:
    UniformSynchronizer(GLuint firstPassProgram, GLuint secondPassProgram)
        : m_program1(firstPassProgram)
        , m_program2(secondPassProgram)
    {
    }

    bool syncUniforms(UniformStore const& store);
    void syncUniformsForce(UniformStore const& store);

private:
    UniformStore m_gpu_uniforms {};
    GLuint m_program1;
    GLuint m_program2;
};

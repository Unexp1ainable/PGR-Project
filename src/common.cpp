#include "common.h"
#include <geGL/StaticCalls.h>
#include <glm/ext/quaternion_geometric.hpp>
#include <glm/gtc/type_ptr.hpp>


using namespace ge::gl;

bool UniformSynchronizer::syncUniforms(const UniformStore &store) {
    bool significantChange = false;

    if (m_gpu_uniforms.cameraMatrix != store.cameraMatrix) {
        m_gpu_uniforms.cameraMatrix = store.cameraMatrix;
        glProgramUniformMatrix4fv(m_program1, glGetUniformLocation(m_program1, "cameraMatrix"), 1, GL_FALSE, glm::value_ptr(store.cameraMatrix));
        significantChange = true;
    }
    if (store.screenWidth != m_gpu_uniforms.screenWidth) {
        m_gpu_uniforms.screenWidth = store.screenWidth;
        glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenWidth"), store.screenWidth);
        significantChange = true;
    }
    if (store.screenHeight != m_gpu_uniforms.screenHeight) {
        m_gpu_uniforms.screenHeight = store.screenHeight;
        glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenHeight"), store.screenHeight);
        significantChange = true;
    }
    if (store.lightPosition != m_gpu_uniforms.lightPosition) {
        m_gpu_uniforms.lightPosition = store.lightPosition;
        glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "lightPosition"), 1, glm::value_ptr(store.lightPosition));
        significantChange = true;
    }
    if (store.roughness != m_gpu_uniforms.roughness) {
        m_gpu_uniforms.roughness = store.roughness;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "roughness"), store.roughness);
        significantChange = true;
    }
    if (store.transparency != m_gpu_uniforms.transparency) {
        m_gpu_uniforms.transparency = store.transparency;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "transparency"), store.transparency);
        significantChange = true;
    }
    if (store.density != m_gpu_uniforms.density) {
        m_gpu_uniforms.density = store.density;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "density"), store.density);
        significantChange = true;
    }
    if (store.n != m_gpu_uniforms.n) {
        m_gpu_uniforms.n = store.n;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "n"), store.n);
        significantChange = true;
    }
    if (store.time != m_gpu_uniforms.time) {
        m_gpu_uniforms.time = store.time;
        glProgramUniform1ui(m_program1, glGetUniformLocation(m_program1, "time"), store.time);
    }
    if (store.chessBoard != m_gpu_uniforms.chessBoard) {
        m_gpu_uniforms.chessBoard = store.chessBoard;
        glProgramUniform1iv(m_program1, glGetUniformLocation(m_program1, "chessBoard"), 8*8*2, store.chessBoard.data());
        significantChange = true;
    }
    return significantChange;
}

void UniformSynchronizer::syncUniformsForce(UniformStore const& store) {
    glProgramUniformMatrix4fv(m_program1, glGetUniformLocation(m_program1, "cameraMatrix"), 1, GL_FALSE, glm::value_ptr(store.cameraMatrix));
    glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenWidth"), store.screenWidth);
    glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenHeight"), store.screenHeight);
    glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "lightPosition"), 1, glm::value_ptr(store.lightPosition));
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "roughness"), store.roughness);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "transparency"), store.transparency);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "density"), store.density);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "n"), store.n);
    glProgramUniform1ui(m_program1, glGetUniformLocation(m_program1, "time"), store.time);
    glProgramUniform1iv(m_program1, glGetUniformLocation(m_program1, "chessBoard"), 8*8*2, store.chessBoard.data());
}

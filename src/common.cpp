#include "common.h"
#include <geGL/StaticCalls.h>
#include <glm/ext/quaternion_geometric.hpp>
#include <glm/gtc/type_ptr.hpp>


using namespace ge::gl;

void UniformSynchronizer::syncUniforms(const UniformStore &store) {
    bool shouldIncrementAccumCounter = true;

    if (m_gpu_uniforms.cameraMatrix != store.cameraMatrix) {
        m_gpu_uniforms.cameraMatrix = store.cameraMatrix;
        glProgramUniformMatrix4fv(m_program1, glGetUniformLocation(m_program1, "cameraMatrix"), 1, GL_FALSE, glm::value_ptr(store.cameraMatrix));
        shouldIncrementAccumCounter = false;
    }
    if (store.screenWidth != m_gpu_uniforms.screenWidth) {
        m_gpu_uniforms.screenWidth = store.screenWidth;
        glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenWidth"), store.screenWidth);
        shouldIncrementAccumCounter = false;
    }
    if (store.screenHeight != m_gpu_uniforms.screenHeight) {
        m_gpu_uniforms.screenHeight = store.screenHeight;
        glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenHeight"), store.screenHeight);
        shouldIncrementAccumCounter = false;
    }
    if (store.lightPosition != m_gpu_uniforms.lightPosition) {
        m_gpu_uniforms.lightPosition = store.lightPosition;
        glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "lightPosition"), 1, glm::value_ptr(store.lightPosition));
        shouldIncrementAccumCounter = false;
    }
    if (store.roughness != m_gpu_uniforms.roughness) {
        m_gpu_uniforms.roughness = store.roughness;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "roughness"), store.roughness);
        shouldIncrementAccumCounter = false;
    }
    if (store.transparency != m_gpu_uniforms.transparency) {
        m_gpu_uniforms.transparency = store.transparency;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "transparency"), store.transparency);
        shouldIncrementAccumCounter = false;
    }
    if (store.density != m_gpu_uniforms.density) {
        m_gpu_uniforms.density = store.density;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "density"), store.density);
        shouldIncrementAccumCounter = false;
    }
    if (store.n != m_gpu_uniforms.n) {
        m_gpu_uniforms.n = store.n;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "n"), store.n);
        shouldIncrementAccumCounter = false;
    }
    if (store.time != m_gpu_uniforms.time) {
        m_gpu_uniforms.time = store.time;
        glProgramUniform1ui(m_program1, glGetUniformLocation(m_program1, "time"), store.time);
    }
    if (store.chessBoard != m_gpu_uniforms.chessBoard) {
        m_gpu_uniforms.chessBoard = store.chessBoard;
        glProgramUniform1iv(m_program1, glGetUniformLocation(m_program1, "chessBoard"), 8*8*2, store.chessBoard.data());
        shouldIncrementAccumCounter = false;
    }
    if (shouldIncrementAccumCounter) {
        m_gpu_uniforms.accumCounter += 1;
        glProgramUniform1ui(m_program1, glGetUniformLocation(m_program1, "accumCounter"), m_gpu_uniforms.accumCounter);
    } else {
        m_gpu_uniforms.accumCounter = 1;
        glProgramUniform1ui(m_program1, glGetUniformLocation(m_program1, "accumCounter"), m_gpu_uniforms.accumCounter);
    }
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

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
    if (store.time != m_gpu_uniforms.time) {
        m_gpu_uniforms.time = store.time;
        glProgramUniform1ui(m_program1, glGetUniformLocation(m_program1, "time"), store.time);
    }
    if (store.chessBoard != m_gpu_uniforms.chessBoard) {
        m_gpu_uniforms.chessBoard = store.chessBoard;
        glProgramUniform1iv(m_program1, glGetUniformLocation(m_program1, "chessBoard"), 8*8*2, store.chessBoard.data());
        significantChange = true;
    }
    if (store.reflectionBounces != m_gpu_uniforms.reflectionBounces) {
        m_gpu_uniforms.reflectionBounces = store.reflectionBounces;
        glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "reflectionBounces"), store.reflectionBounces);
        significantChange = true;
    }
    if (store.refractionBounces != m_gpu_uniforms.refractionBounces) {
        m_gpu_uniforms.refractionBounces = store.refractionBounces;
        glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "refractionBounces"), store.refractionBounces);
        significantChange = true;
    }
    if (store.shadowRays != m_gpu_uniforms.shadowRays) {
        m_gpu_uniforms.shadowRays = store.shadowRays;
        glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "shadowRays"), store.shadowRays);
        significantChange = true;
    }


    if (store.blackRoughness != m_gpu_uniforms.blackRoughness) {
        m_gpu_uniforms.blackRoughness = store.blackRoughness;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackRoughness"), store.blackRoughness);
        significantChange = true;
    }
    if (store.blackTransparency != m_gpu_uniforms.blackTransparency) {
        m_gpu_uniforms.blackTransparency = store.blackTransparency;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackTransparency"), store.blackTransparency);
        significantChange = true;
    }
    if (store.blackDensity != m_gpu_uniforms.blackDensity) {
        m_gpu_uniforms.blackDensity = store.blackDensity;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackDensity"), store.blackDensity);
        significantChange = true;
    }
    if (store.blackN != m_gpu_uniforms.blackN) {
        m_gpu_uniforms.blackN = store.blackN;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackN"), store.blackN);
        significantChange = true;
    }
    if (store.blackColor != m_gpu_uniforms.blackColor) {
        m_gpu_uniforms.blackColor = store.blackColor;
        glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "blackColor"), 1, glm::value_ptr(store.blackColor));
        significantChange = true;
    }

    if (store.whiteRoughness != m_gpu_uniforms.whiteRoughness) {
        m_gpu_uniforms.whiteRoughness = store.whiteRoughness;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteRoughness"), store.whiteRoughness);
        significantChange = true;
    }
    if (store.whiteTransparency != m_gpu_uniforms.whiteTransparency) {
        m_gpu_uniforms.whiteTransparency = store.whiteTransparency;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteTransparency"), store.whiteTransparency);
        significantChange = true;
    }
    if (store.whiteDensity != m_gpu_uniforms.whiteDensity) {
        m_gpu_uniforms.whiteDensity = store.whiteDensity;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteDensity"), store.whiteDensity);
        significantChange = true;
    }
    if (store.whiteN != m_gpu_uniforms.whiteN) {
        m_gpu_uniforms.whiteN = store.whiteN;
        glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteN"), store.whiteN);
        significantChange = true;
    }
    if (store.whiteColor != m_gpu_uniforms.whiteColor) {
        m_gpu_uniforms.whiteColor = store.whiteColor;
        glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "whiteColor"), 1, glm::value_ptr(store.whiteColor));
        significantChange = true;
    }
    
    return significantChange;
}

void UniformSynchronizer::syncUniformsForce(UniformStore const& store) {
    glProgramUniformMatrix4fv(m_program1, glGetUniformLocation(m_program1, "cameraMatrix"), 1, GL_FALSE, glm::value_ptr(store.cameraMatrix));
    glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenWidth"), store.screenWidth);
    glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "screenHeight"), store.screenHeight);
    glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "lightPosition"), 1, glm::value_ptr(store.lightPosition));
    glProgramUniform1ui(m_program1, glGetUniformLocation(m_program1, "time"), store.time);
    glProgramUniform1iv(m_program1, glGetUniformLocation(m_program1, "chessBoard"), 8*8*2, store.chessBoard.data());
    glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "reflectionBounces"), store.reflectionBounces);
    glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "refractionBounces"), store.refractionBounces);
    glProgramUniform1i(m_program1, glGetUniformLocation(m_program1, "shadowRays"), store.shadowRays);

    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackRoughness"), store.blackRoughness);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackTransparency"), store.blackTransparency);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackDensity"), store.blackDensity);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "blackN"), store.blackN);
    glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "blackColor"), 1, glm::value_ptr(store.blackColor));

    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteRoughness"), store.whiteRoughness);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteTransparency"), store.whiteTransparency);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteDensity"), store.whiteDensity);
    glProgramUniform1f(m_program1, glGetUniformLocation(m_program1, "whiteN"), store.whiteN);
    glProgramUniform3fv(m_program1, glGetUniformLocation(m_program1, "whiteColor"), 1, glm::value_ptr(store.whiteColor));
}

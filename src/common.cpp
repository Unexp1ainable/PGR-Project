#include "common.h"
#include <geGL/StaticCalls.h>
#include <glm/ext/quaternion_geometric.hpp>
#include <glm/gtc/type_ptr.hpp>


using namespace ge::gl;

void UniformSynchronizer::syncUniforms(const UniformStore &store) {
    if (m_gpu_uniforms.cameraMatrix != store.cameraMatrix) {
        m_gpu_uniforms.cameraMatrix = store.cameraMatrix;
        glProgramUniformMatrix4fv(m_program, glGetUniformLocation(m_program, "cameraMatrix"), 1, GL_FALSE, glm::value_ptr(store.cameraMatrix));
    }
    if (store.screenWidth != m_gpu_uniforms.screenWidth) {
        m_gpu_uniforms.screenWidth = store.screenWidth;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenWidth"), store.screenWidth);
    }
    if (store.screenHeight != m_gpu_uniforms.screenHeight) {
        m_gpu_uniforms.screenHeight = store.screenHeight;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenHeight"), store.screenHeight);
    }
    if (store.lightPosition != m_gpu_uniforms.lightPosition) {
        m_gpu_uniforms.lightPosition = store.lightPosition;
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "lightPosition"), 1, glm::value_ptr(store.lightPosition));
    }
    if (store.roughness != m_gpu_uniforms.roughness) {
        m_gpu_uniforms.roughness = store.roughness;
        glProgramUniform1f(m_program, glGetUniformLocation(m_program, "roughness"), store.roughness);
    }
    if (store.fresnel != m_gpu_uniforms.fresnel) {
        m_gpu_uniforms.fresnel = store.fresnel;
        glProgramUniform1f(m_program, glGetUniformLocation(m_program, "fresnel"), store.fresnel);
    }
    if (store.density != m_gpu_uniforms.density) {
        m_gpu_uniforms.density = store.density;
        glProgramUniform1f(m_program, glGetUniformLocation(m_program, "density"), store.density);
    }
}

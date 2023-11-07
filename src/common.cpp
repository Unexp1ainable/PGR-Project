#include "common.h"
#include <geGL/StaticCalls.h>
#include <glm/gtc/type_ptr.hpp>


using namespace ge::gl;

void UniformSynchronizer::syncUniforms(const UniformStore &store) {

    if (store.screenWidth != m_gpu_uniforms.screenWidth) {
        m_gpu_uniforms.screenWidth = store.screenWidth;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenWidth"), m_gpu_uniforms.screenWidth);
    }
    if (store.screenHeight != m_gpu_uniforms.screenHeight) {
        m_gpu_uniforms.screenHeight = store.screenHeight;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenHeight"), m_gpu_uniforms.screenHeight);
    }
    if (store.lightPosition != m_gpu_uniforms.lightPosition) {
        m_gpu_uniforms.lightPosition = store.lightPosition;
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "lightPosition"), 1, glm::value_ptr(m_gpu_uniforms.lightPosition));
    }
}

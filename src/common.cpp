#include "common.h"
#include <geGL/StaticCalls.h>
#include <glm/ext/quaternion_geometric.hpp>
#include <glm/gtc/type_ptr.hpp>


using namespace ge::gl;

void UniformSynchronizer::syncUniforms(const UniformStore &store, glm::mat4 view) {
    bool viewChanged = false;
    if (m_view != view) {
        m_view = view;
        glProgramUniformMatrix4fv(m_program, glGetUniformLocation(m_program, "cameraMatrix"), 1, GL_FALSE, glm::value_ptr(view));
    }

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
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "lightPosition"), 1, glm::value_ptr(store.lightPosition));
    }
    if (store.spherePosition != m_gpu_uniforms.spherePosition) {
        m_gpu_uniforms.spherePosition = store.spherePosition;
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "sphere.center"), 1, glm::value_ptr(store.spherePosition));
    }
}

#include "common.h"
#include <geGL/StaticCalls.h>
#include <glm/gtc/type_ptr.hpp>


using namespace ge::gl;

void UniformSynchronizer::syncUniforms(const UniformStore &store, glm::mat4 view) {
    bool viewChanged = false;
    if (m_view != view) {
        m_view = view;
        viewChanged = true;
    }

    if (store.screenWidth != m_gpu_uniforms.screenWidth || viewChanged) {
        m_gpu_uniforms.screenWidth = store.screenWidth;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenWidth"), m_gpu_uniforms.screenWidth);
    }
    if (store.screenHeight != m_gpu_uniforms.screenHeight || viewChanged) {
        m_gpu_uniforms.screenHeight = store.screenHeight;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenHeight"), m_gpu_uniforms.screenHeight);
    }
    if (store.lightPosition != m_gpu_uniforms.lightPosition || viewChanged) {
        m_gpu_uniforms.lightPosition = store.lightPosition;
        glm::vec3 lightPos = glm::vec3(glm::vec4(m_gpu_uniforms.lightPosition, 1.0));
        glm::vec3 lightPos2 = glm::vec3(view * glm::vec4(m_gpu_uniforms.lightPosition, 1.0));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "lightPosition"), 1, glm::value_ptr(lightPos));
    }
    if (store.spherePosition != m_gpu_uniforms.spherePosition || viewChanged) {
        m_gpu_uniforms.spherePosition = store.spherePosition;
        glm::vec3 spherePos = glm::vec3(view * glm::vec4(m_gpu_uniforms.spherePosition, 1.0));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "sphere.center"), 1, glm::value_ptr(spherePos));
    }
}

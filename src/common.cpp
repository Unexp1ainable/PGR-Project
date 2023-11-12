#include "common.h"
#include <geGL/StaticCalls.h>
#include <glm/ext/quaternion_geometric.hpp>
#include <glm/gtc/type_ptr.hpp>


using namespace ge::gl;

void UniformSynchronizer::syncUniforms(const UniformStore &store, glm::mat4 view) {
    bool viewChanged = false;
    if (m_view != view) {
        m_view = view;
        viewChanged = true;
    }

    if (store.screenWidth != m_gpu_uniforms.screenWidth) {
        m_gpu_uniforms.screenWidth = store.screenWidth;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenWidth"), m_gpu_uniforms.screenWidth);
    }
    if (store.screenHeight != m_gpu_uniforms.screenHeight) {
        m_gpu_uniforms.screenHeight = store.screenHeight;
        glProgramUniform1i(m_program, glGetUniformLocation(m_program, "screenHeight"), m_gpu_uniforms.screenHeight);
    }
    if (store.lightPosition != m_gpu_uniforms.lightPosition || viewChanged) {
        m_gpu_uniforms.lightPosition = store.lightPosition;
        glm::vec3 lightPos = glm::vec3(view * glm::vec4(m_gpu_uniforms.lightPosition, 1.0));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "lightPosition"), 1, glm::value_ptr(lightPos));
    }
    if (store.spherePosition != m_gpu_uniforms.spherePosition || viewChanged) {
        m_gpu_uniforms.spherePosition = store.spherePosition;
        glm::vec3 spherePos = glm::vec3(view * glm::vec4(m_gpu_uniforms.spherePosition, 1.0));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "sphere.center"), 1, glm::value_ptr(spherePos));
    }
    if (store.cylinderPosition != m_gpu_uniforms.cylinderPosition || viewChanged) {
        m_gpu_uniforms.cylinderPosition = store.cylinderPosition;
        glm::vec3 cylinderPos = glm::vec3(view * glm::vec4(m_gpu_uniforms.cylinderPosition, 1.0));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "cylinder.center"), 1, glm::value_ptr(cylinderPos));
    }
    if (store.cylinderDirection != m_gpu_uniforms.cylinderDirection || viewChanged) {
        m_gpu_uniforms.cylinderDirection = store.cylinderDirection;
        glm::vec3 cylinderDirection = glm::normalize(glm::vec3(view * glm::vec4(m_gpu_uniforms.cylinderDirection, 0.0)));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "cylinder.direction"), 1, glm::value_ptr(cylinderDirection));
    }
    if (store.planePosition != m_gpu_uniforms.planePosition || viewChanged) {
        m_gpu_uniforms.planePosition = store.planePosition;
        glm::vec3 planePosition = glm::vec3(view * glm::vec4(m_gpu_uniforms.planePosition, 1.0));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "plane.center"), 1, glm::value_ptr(planePosition));
    }
    if (store.planeNormal != m_gpu_uniforms.planeNormal || viewChanged) {
        m_gpu_uniforms.planeNormal = store.planeNormal;
        glm::vec3 planeNormal = glm::normalize(glm::vec3(view * glm::vec4(m_gpu_uniforms.planeNormal, 0.0)));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "plane.normal"), 1, glm::value_ptr(planeNormal));
    }
    if (store.planeDirection != m_gpu_uniforms.planeDirection || viewChanged) {
        m_gpu_uniforms.planeDirection = store.planeDirection;
        glm::vec3 planeDirection = glm::normalize(glm::vec3(view * glm::vec4(m_gpu_uniforms.planeDirection, 0.0)));
        glProgramUniform3fv(m_program, glGetUniformLocation(m_program, "plane.direction"), 1, glm::value_ptr(planeDirection));
    }
}

#pragma once

#include <geGL/Generated/OpenGLTypes.h>
#include <glm/glm.hpp>

#include <string>
#include <vector>
#include <chrono>


// rectangle through the whole screen
const std::vector<glm::vec3> VAO_DATA = {
    { -1., -1., -1. },
    { +1., -1., -1. },
    { -1., +1., -1. },
    { +1., +1., -1. },
};


class OpenGLContext {
public:
    OpenGLContext();

    void useRenderProgram() const;

    GLuint getFirstPassProgram() const { return m_renderPrg; }
    GLuint getSecondPassProgram() const { return m_showTexturePrg; }
    void showTexture(bool significantChange);
    // GLuint getVAO() const { return m_vao; }

protected:
    GLuint createShader(GLenum type, std::string const& src);
    GLuint createProgram(std::vector<GLuint> const& shaders);
    GLuint createVAO();
    void createGBuffer();
    void createSkybox();

private:
    GLuint m_vao;
    GLuint m_vbo;
    GLuint m_renderPrg;
    GLuint m_showTexturePrg;
    GLuint m_fbo;
    GLuint m_shadowTexture;
    GLuint m_reflectionTexture;
    GLuint m_refractionTexture;
    GLuint m_ambientTexture;
    GLuint m_primaryTexture;
    GLuint m_skybox;

    std::chrono::high_resolution_clock::time_point m_lastTime;
};

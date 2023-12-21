#pragma once

#include <geGL/Generated/OpenGLTypes.h>
#include <glm/glm.hpp>

#include <string>
#include <vector>


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

    GLuint getRenderProgram() const { return m_renderPrg; }
    void showTexture();
    // GLuint getVAO() const { return m_vao; }

protected:
    GLuint createShader(GLenum type, std::string const& src);
    GLuint createProgram(std::vector<GLuint> const& shaders);
    GLuint createVAO();

private:
    GLuint m_vao;
    GLuint m_vbo;
    GLuint m_renderPrg;
    GLuint m_showTexturePrg;
    GLuint m_fbo;
    GLuint m_shadowTexture;
    GLuint m_specDiffTexture;
    GLuint m_ambientTexture;
};

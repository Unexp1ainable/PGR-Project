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


GLuint createShader(GLenum type, std::string const& src);
GLuint createProgram(std::vector<GLuint> const& shaders);
GLuint createVAO();

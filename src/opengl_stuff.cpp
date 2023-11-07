#include "opengl_stuff.h"

#include <geGL/StaticCalls.h>

#include <iostream>


using namespace ge::gl;

GLuint createShader(GLenum type, std::string const& src)
{
    auto id            = glCreateShader(type);
    char const* srcs[] = { src.c_str() };
    glShaderSource(id, 1, srcs, nullptr);
    glCompileShader(id);

    // get compilation status
    int compileStatus;
    glGetShaderiv(id, GL_COMPILE_STATUS, &compileStatus);
    if (compileStatus != GL_TRUE) {
        // get message info length
        GLint msgLen;
        glGetShaderiv(id, GL_INFO_LOG_LENGTH, &msgLen);
        auto message = std::string(msgLen, ' ');
        // get message
        glGetShaderInfoLog(id, msgLen, nullptr, message.data());
        std::cerr << "Shader compilation error\n" << message << std::endl;
    }
    return id;
}

GLuint createProgram(std::vector<GLuint> const& shaders)
{
    auto id = glCreateProgram();
    for (auto const& shader : shaders)
        glAttachShader(id, shader);
    glLinkProgram(id);

    // get link status
    GLint linkStatus;
    glGetProgramiv(id, GL_LINK_STATUS, &linkStatus);
    if (linkStatus != GL_TRUE) {
        // get message info length
        GLint msgLen;
        glGetProgramiv(id, GL_INFO_LOG_LENGTH, &msgLen);
        auto message = std::string(msgLen, ' ');
        glGetProgramInfoLog(id, msgLen, nullptr, message.data());
        std::cerr << message << std::endl;
    }

    return id;
}

GLuint createVAO()
{
    GLuint vbo; // buffer handle
    glCreateBuffers(1, &vbo);
    // allocate buffer and upload data
    glNamedBufferData(vbo, VAO_DATA.size() * sizeof(glm::vec3), VAO_DATA.data(), GL_DYNAMIC_DRAW);

    GLuint vao;
    glCreateVertexArrays(1, &vao);
    glVertexArrayAttribBinding(vao, 0, 0);
    glEnableVertexArrayAttrib(vao, 0);
    glVertexArrayAttribFormat(vao, 0, 3, GL_FLOAT, GL_FALSE, 0);
    glVertexArrayVertexBuffer(vao, 0, vbo, 0, sizeof(glm::vec3));

    return vao;
}

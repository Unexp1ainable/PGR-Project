#include "opengl_stuff.h"
#include "constants.h"
#include "generated/shaders/fragment_shader.h"
#include "generated/shaders/show_texture.h"
#include "generated/shaders/vertex_shader.h"
#include <chrono>
#define STB_IMAGE_IMPLEMENTATION
#include "stb/stb_image.h"


#include <geGL/StaticCalls.h>

#include <fstream>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>


using namespace ge::gl;


std::string loadFile(std::string path)
{
    std::ifstream file(path);
    if (!file.is_open()) {
        throw std::runtime_error("Failed to open file: " + path);
    }
    std::stringstream buffer;
    buffer << file.rdbuf();
    return buffer.str();
}

// https://gist.github.com/liam-middlebrook/c52b069e4be2d87a6d2f
void GLDebugMessageCallback(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* msg, const void* data)
{
    std::string _source;
    std::string _type;
    std::string _severity;

    switch (source) {
        case GL_DEBUG_SOURCE_API:
            _source = "API";
            break;

        case GL_DEBUG_SOURCE_WINDOW_SYSTEM:
            _source = "WINDOW SYSTEM";
            break;

        case GL_DEBUG_SOURCE_SHADER_COMPILER:
            _source = "SHADER COMPILER";
            break;

        case GL_DEBUG_SOURCE_THIRD_PARTY:
            _source = "THIRD PARTY";
            break;

        case GL_DEBUG_SOURCE_APPLICATION:
            _source = "APPLICATION";
            break;

        case GL_DEBUG_SOURCE_OTHER:
            _source = "UNKNOWN";
            break;

        default:
            _source = "UNKNOWN";
            break;
    }

    switch (type) {
        case GL_DEBUG_TYPE_ERROR:
            _type = "ERROR";
            break;

        case GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR:
            _type = "DEPRECATED BEHAVIOR";
            break;

        case GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR:
            _type = "UDEFINED BEHAVIOR";
            break;

        case GL_DEBUG_TYPE_PORTABILITY:
            _type = "PORTABILITY";
            break;

        case GL_DEBUG_TYPE_PERFORMANCE:
            _type = "PERFORMANCE";
            break;

        case GL_DEBUG_TYPE_OTHER:
            _type = "OTHER";
            break;

        case GL_DEBUG_TYPE_MARKER:
            _type = "MARKER";
            break;

        default:
            _type = "UNKNOWN";
            break;
    }

    switch (severity) {
        case GL_DEBUG_SEVERITY_HIGH:
            _severity = "HIGH";
            break;

        case GL_DEBUG_SEVERITY_MEDIUM:
            _severity = "MEDIUM";
            break;

        case GL_DEBUG_SEVERITY_LOW:
            _severity = "LOW";
            break;

        case GL_DEBUG_SEVERITY_NOTIFICATION:
            _severity = "NOTIFICATION";
            break;

        default:
            _severity = "UNKNOWN";
            break;
    }

    printf("%d: %s of %s severity, raised from %s: %s\n", id, _type.c_str(), _severity.c_str(), _source.c_str(), msg);
}


void OpenGLContext::createGBuffer()
{
    glGenFramebuffers(1, &m_fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, m_fbo);

    // Create the gbuffer textures
    glGenTextures(1, &m_shadowTexture);
    glBindTexture(GL_TEXTURE_2D, m_shadowTexture);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB32F, MAX_WINDOW_WIDTH, MAX_WINDOW_HEIGHT, 0, GL_RGB, GL_FLOAT, NULL);
    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, m_shadowTexture, 0);

    glGenTextures(1, &m_reflectionTexture);
    glBindTexture(GL_TEXTURE_2D, m_reflectionTexture);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32F, MAX_WINDOW_WIDTH, MAX_WINDOW_HEIGHT, 0, GL_RGBA, GL_FLOAT, NULL);
    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT1, m_reflectionTexture, 0);

    glGenTextures(1, &m_ambientTexture);
    glBindTexture(GL_TEXTURE_2D, m_ambientTexture);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32F, MAX_WINDOW_WIDTH, MAX_WINDOW_HEIGHT, 0, GL_RGBA, GL_FLOAT, NULL);
    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT2, m_ambientTexture, 0);

    glGenTextures(1, &m_refractionTexture);
    glBindTexture(GL_TEXTURE_2D, m_refractionTexture);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32F, MAX_WINDOW_WIDTH, MAX_WINDOW_HEIGHT, 0, GL_RGBA, GL_FLOAT, NULL);
    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT3, m_refractionTexture, 0);

    glGenTextures(1, &m_primaryTexture);
    glBindTexture(GL_TEXTURE_2D, m_primaryTexture);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32F, MAX_WINDOW_WIDTH, MAX_WINDOW_HEIGHT, 0, GL_RGBA, GL_FLOAT, NULL);
    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT4, m_primaryTexture, 0);

    GLenum drawBuffers[] = { GL_COLOR_ATTACHMENT0, GL_COLOR_ATTACHMENT1, GL_COLOR_ATTACHMENT2, GL_COLOR_ATTACHMENT3, GL_COLOR_ATTACHMENT4 };
    glDrawBuffers(5, drawBuffers);

    auto status = glCheckNamedFramebufferStatus(m_fbo, GL_FRAMEBUFFER);
    if (glCheckNamedFramebufferStatus(m_fbo, GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) {
        std::cerr << "framebuffer incomplete " << status << std::endl;
    }
}

unsigned int loadCubemap(std::vector<std::string> faces)
{
    unsigned int textureID;
    glGenTextures(1, &textureID);
    glBindTexture(GL_TEXTURE_CUBE_MAP, textureID);

    int width, height, nrChannels;
    for (unsigned int i = 0; i < faces.size(); i++) {
        unsigned char* data = stbi_load(faces[i].c_str(), &width, &height, &nrChannels, 0);
        if (data) {
            glTexImage2D(GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
            stbi_image_free(data);
        } else {
            std::cout << "Cubemap tex failed to load at path: " << faces[i] << std::endl;
            stbi_image_free(data);
        }
    }
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);

    return textureID;
}

void OpenGLContext::createSkybox()
{
    std::vector<std::string> textures_faces = {
        "../resources/posx.jpg", "../resources/negx.jpg", "../resources/posy.jpg",
        "../resources/negy.jpg", "../resources/posz.jpg", "../resources/negz.jpg",
    };
    m_skybox = loadCubemap(textures_faces);
}

OpenGLContext::OpenGLContext()
{
    #ifndef NDEBUG
    glEnable(GL_DEBUG_OUTPUT);
    glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);
    glDebugMessageCallback(GLDebugMessageCallback, NULL);
    #endif

    std::string vSrc  = VERTEX_SHADER_SOURCE;
    std::string fSrc  = FRAGMENT_SHADER_SOURCE;
    std::string tfSrc = SHOW_TEXTURE_SOURCE;

    // TODO remove
    vSrc  = loadFile("/mnt/d/VUT/MIT/3semester/PGR/projekt/src/shaders/vertex_shader.vs");
    fSrc  = loadFile("/mnt/d/VUT/MIT/3semester/PGR/projekt/src/shaders/first_pass.fs");
    tfSrc = loadFile("/mnt/d/VUT/MIT/3semester/PGR/projekt/src/shaders/second_pass.fs");

    auto vShader  = createShader(GL_VERTEX_SHADER, vSrc);
    auto fShader  = createShader(GL_FRAGMENT_SHADER, fSrc);
    auto tfShader = createShader(GL_FRAGMENT_SHADER, tfSrc);

    createGBuffer();
    createSkybox();

    m_renderPrg      = createProgram({ vShader, fShader });
    m_showTexturePrg = createProgram({ vShader, tfShader });
    m_vao            = createVAO();
}

void OpenGLContext::useFirstPassProgram() const
{
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, m_fbo);
    glClear(GL_COLOR_BUFFER_BIT);
    glBindVertexArray(m_vao);
    glUseProgram(m_renderPrg);

    glActiveTexture(GL_TEXTURE5);
    glBindTexture(GL_TEXTURE_CUBE_MAP, m_skybox);
    glUniform1i(glGetUniformLocation(m_renderPrg, "skybox"), 5);

    glDrawArrays(GL_TRIANGLE_STRIP, 0, VAO_DATA.size());
    glBindVertexArray(0);
}

void OpenGLContext::useSecondPassProgram(bool significantChange)
{
    glUseProgram(m_showTexturePrg);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
    // glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glReadBuffer(GL_COLOR_ATTACHMENT2);

    // Bind the texture to texture unit 0
    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, m_reflectionTexture);
    // Bind the shader program and set the texture uniform
    glUniform1i(glGetUniformLocation(m_showTexturePrg, "textureReflection"), 0);

    // Bind the texture to texture unit 1
    glActiveTexture(GL_TEXTURE1);
    glBindTexture(GL_TEXTURE_2D, m_ambientTexture);
    // Bind the shader program and set the texture uniform
    glUniform1i(glGetUniformLocation(m_showTexturePrg, "textureAmbient"), 1);

    // Bind the texture to texture unit 2
    glActiveTexture(GL_TEXTURE2);
    glBindTexture(GL_TEXTURE_2D, m_shadowTexture);
    // Bind the shader program and set the texture uniform
    glUniform1i(glGetUniformLocation(m_showTexturePrg, "textureShadows"), 2);

    // Bind the texture to texture unit 3
    glActiveTexture(GL_TEXTURE3);
    glBindTexture(GL_TEXTURE_2D, m_refractionTexture);
    // Bind the shader program and set the texture uniform
    glUniform1i(glGetUniformLocation(m_showTexturePrg, "textureRefraction"), 3);

    // Bind the texture to texture unit 4
    glActiveTexture(GL_TEXTURE4);
    glBindTexture(GL_TEXTURE_2D, m_primaryTexture);
    // Bind the shader program and set the texture uniform
    glUniform1i(glGetUniformLocation(m_showTexturePrg, "texturePrimary"), 4);


    if (!significantChange) {
        if (std::chrono::high_resolution_clock::now() - m_lastTime > std::chrono::milliseconds(100)) {
            glEnable(GL_BLEND);
            glBlendFunc(GL_CONSTANT_ALPHA, GL_ONE_MINUS_CONSTANT_ALPHA);
            glBlendColor(1.0f, 1.0f, 1.0f, 0.2);
        }
    } else {
        m_lastTime = std::chrono::high_resolution_clock::now();
    }


    glBindVertexArray(m_vao);

    glDrawArrays(GL_TRIANGLE_STRIP, 0, VAO_DATA.size());
    glBindTexture(GL_TEXTURE_2D, 0);
    glBindVertexArray(0);

    glDisable(GL_BLEND);
}

GLuint OpenGLContext::createShader(GLenum type, std::string const& src)
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

GLuint OpenGLContext::createProgram(std::vector<GLuint> const& shaders)
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

GLuint OpenGLContext::createVAO()
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

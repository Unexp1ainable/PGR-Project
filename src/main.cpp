#include <fstream>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

#include "common.h"
#include <SDL.h>
#include <geGL/Generated/OpenGLConstants.h>
#include <geGL/StaticCalls.h>
#include <geGL/geGL.h>
#include <glm/glm.hpp>
#include <imgui.h>
#include "imgui_impl_opengl3.h"
#include <imgui_impl_sdl2.h>

#include "generated/shaders/fragment_shader.h"
#include "generated/shaders/vertex_shader.h"
#include "imgui_stuff.h"
#include "opengl_stuff.h"
#include "sdl_stuff.h"

using namespace ge::gl;

void mainloop(SDL_Window* window, GLuint vao, GLuint prg)
{
    UniformStore store;
    bool running = true;
    while (running) {
        SDL_Event event;
        while (SDL_PollEvent(&event)) {
            if (event.type == SDL_QUIT)
                running = false;
            // (Where your code calls SDL_PollEvent())
            ImGui_ImplSDL2_ProcessEvent(&event); // Forward your event to backend
        }


        // (After event loop)
        // Start the Dear ImGui frame
        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplSDL2_NewFrame();
        ImGui::NewFrame();
        // ImGui::ShowDemoWindow(); // Show demo window! :)
        drawGui(store);


        glClearColor(0, 0.5, 0, 1);
        glClear(GL_COLOR_BUFFER_BIT);

        glBindVertexArray(vao);

        glUseProgram(prg);
        glDrawArrays(GL_TRIANGLE_STRIP, 0, VAO_DATA.size());

        glBindVertexArray(0);

        ImGui::Render();
        ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
        SDL_GL_SwapWindow(window);
    }
}


// load file specified by path and return content as string
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

int main(int argc, char* argv[])
{
    ge::gl::init();

    auto sdlGuard = SDL_Guard();
    auto window  = sdlGuard.createWindow();
    auto context = sdlGuard.getContext();

    auto imGuiGuard = ImGui_Guard(window, context);

    std::string vSrc = VERTEX_SHADER_SOURCE;
    std::string fSrc = FRAGMENT_SHADER_SOURCE;

    vSrc = loadFile("/mnt/d/VUT/MIT/3semester/PGR/projekt/src/shaders/vertex_shader.vs");
    fSrc = loadFile("/mnt/d/VUT/MIT/3semester/PGR/projekt/src/shaders/fragment_shader.fs");

    auto vShader = createShader(GL_VERTEX_SHADER, vSrc);
    auto fShader = createShader(GL_FRAGMENT_SHADER, fSrc);

    GLuint prg = createProgram({ vShader, fShader });

    auto vao = createVAO();
    mainloop(window, vao, prg);

    return 0;
}

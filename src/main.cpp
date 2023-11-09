#include <SDL2/SDL_events.h>
#include <fstream>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>

#include "SDL_keycode.h"
#include "SDL_mouse.h"
#include "SDL_scancode.h"
#include "common.h"
#include "imgui_impl_opengl3.h"
#include <SDL.h>
#include <geGL/Generated/OpenGLConstants.h>
#include <geGL/StaticCalls.h>
#include <geGL/geGL.h>
#include <glm/glm.hpp>
#include <imgui.h>
#include <imgui_impl_sdl2.h>

#include "camera/freelook_camera.h"
#include "generated/shaders/fragment_shader.h"
#include "generated/shaders/vertex_shader.h"
#include "imgui_stuff.h"
#include "opengl_stuff.h"
#include "sdl_stuff.h"

using namespace ge::gl;

void mainloop(SDL_Window* window, GLuint vao, GLuint prg)
{
    UniformStore uniforms;
    UniformSynchronizer synchronizer(prg);
    FreeLookCamera camera {};
    float sensitivity = 0.01;
    bool running      = true;
    bool pressed      = false;

    while (running) {
        // SDL_PumpEvents();
        const Uint8* keystate = SDL_GetKeyboardState(NULL);

        // continuous-response keys
        if (keystate[SDL_SCANCODE_W]) {
            camera.forward(-sensitivity);
        }
        if (keystate[SDL_SCANCODE_A]) {
            camera.left(sensitivity);
        }
        if (keystate[SDL_SCANCODE_S]) {
            camera.back(-sensitivity);
        }
        if (keystate[SDL_SCANCODE_D]) {
            camera.right(sensitivity);
        }
        if (keystate[SDL_SCANCODE_SPACE]) {
            camera.up(sensitivity);
        }
        if (keystate[SDL_SCANCODE_LSHIFT]) {
            camera.down(sensitivity);
        }

        SDL_Event event;
        while (SDL_PollEvent(&event)) {
            ImGui_ImplSDL2_ProcessEvent(&event); // Forward event to backend

            if (ImGui::GetIO().WantCaptureMouse) {
                continue;
            }
            if (event.type == SDL_QUIT) {
                running = false;
            }
            if (event.type == SDL_MOUSEBUTTONDOWN) {
                if (event.button.button == SDL_BUTTON_LEFT) {
                    pressed = true;
                }
                if (event.button.button == SDL_BUTTON_RIGHT) { }
            } else if (event.type == SDL_MOUSEBUTTONUP) {
                if (event.button.button == SDL_BUTTON_LEFT) {
                    pressed = false;
                }
                if (event.button.button == SDL_BUTTON_RIGHT) { }
            } else if (event.type == SDL_MOUSEMOTION && event.motion.state & SDL_BUTTON_LMASK) {
                auto xrel = event.motion.xrel / 500.;
                auto yrel = event.motion.yrel / 500.;
                camera.addXAngle(-yrel);
                camera.addYAngle(-xrel);
            }
        }

        // Update the uniforms
        synchronizer.syncUniforms(uniforms, camera.getView());

        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplSDL2_NewFrame();
        ImGui::NewFrame();
        // ImGui::ShowDemoWindow(); // Show demo window! :)
        drawGui(uniforms);


        glClearColor(0, 0, 0, 1);
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
    auto window   = sdlGuard.createWindow();
    auto context  = sdlGuard.getContext();

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

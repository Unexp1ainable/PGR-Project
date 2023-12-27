#include <SDL2/SDL_events.h>
#include <chrono>
#include <glm/matrix.hpp>
#include <iostream>
#include <sstream>
#include <string>
#include <sys/types.h>
#include <vector>

#include "SDL_keycode.h"
#include "SDL_mouse.h"
#include "SDL_scancode.h"
#include "SDL_video.h"
#include "uniforms.h"
#include "imgui_impl_opengl3.h"
#include <SDL.h>
#include <geGL/Generated/OpenGLConstants.h>
#include <geGL/StaticCalls.h>
#include <geGL/geGL.h>
#include <glm/glm.hpp>
#include <imgui.h>
#include <imgui_impl_sdl2.h>

#include "camera/freelook_camera.h"
#include "imgui_stuff.h"
#include "opengl_stuff.h"
#include "sdl_stuff.h"


using namespace ge::gl;

constexpr const float SENSITIVITY = 0.01f;

void resizeWindow(int width, int height, UniformStore& uniforms)
{
    glViewport(0, 0, width, height);
    uniforms.screenWidth  = width;
    uniforms.screenHeight = height;
}

void processInput(bool& running, FreeLookCamera& camera, UniformStore& uniforms)
{
    const Uint8* keystate = SDL_GetKeyboardState(NULL);

    // continuous-response keys
    if (keystate[SDL_SCANCODE_W]) {
        camera.forward(-SENSITIVITY);
    }
    if (keystate[SDL_SCANCODE_A]) {
        camera.left(SENSITIVITY);
    }
    if (keystate[SDL_SCANCODE_S]) {
        camera.back(-SENSITIVITY);
    }
    if (keystate[SDL_SCANCODE_D]) {
        camera.right(SENSITIVITY);
    }
    if (keystate[SDL_SCANCODE_SPACE]) {
        camera.up(SENSITIVITY);
    }
    if (keystate[SDL_SCANCODE_LSHIFT]) {
        camera.down(SENSITIVITY);
    }

    SDL_Event event;
    while (SDL_PollEvent(&event)) {
        ImGui_ImplSDL2_ProcessEvent(&event); // Forward event to backend

        if (ImGui::GetIO().WantCaptureMouse) {
            continue;
        }

        switch (event.type) {
            case SDL_QUIT:
                running = false;
                break;
            case SDL_MOUSEBUTTONDOWN:
                if (event.button.button == SDL_BUTTON_LEFT) { }
                if (event.button.button == SDL_BUTTON_RIGHT) { }
                break;
            case SDL_MOUSEBUTTONUP:
                if (event.button.button == SDL_BUTTON_LEFT) { }
                if (event.button.button == SDL_BUTTON_RIGHT) { }
                break;
            case SDL_MOUSEMOTION:
                if (event.motion.state & SDL_BUTTON_LMASK) {
                    auto xrel = event.motion.xrel / 500.;
                    auto yrel = event.motion.yrel / 500.;
                    camera.addXAngle(-yrel);
                    camera.addYAngle(-xrel);
                }
                break;
            case SDL_WINDOWEVENT:
                if (event.window.event == SDL_WINDOWEVENT_SIZE_CHANGED) {
                    resizeWindow(event.window.data1, event.window.data2, uniforms);
                }
                break;
        }
    }

    uniforms.cameraMatrix = glm::inverse(camera.getView());
}

void mainloop(SDL_Window* window, OpenGLContext& oglCtx)
{
    UniformStore uniforms;
    UniformSynchronizer synchronizer(oglCtx.getFirstPassProgram(), oglCtx.getSecondPassProgram());
    FreeLookCamera camera {};
    bool running = true;

    // Setup time
    auto start = std::chrono::high_resolution_clock::now();
    auto end   = std::chrono::high_resolution_clock::now();
    
    // ensure that uniforms on gpu and cpu are the same
    synchronizer.syncUniformsForce(uniforms);

    while (running) {
        end                                    = std::chrono::high_resolution_clock::now();
        std::chrono::duration<double> duration = end - start;
        float fps                              = 1 / duration.count();
        start                                  = std::chrono::high_resolution_clock::now();
        processInput(running, camera, uniforms);
        uniforms.time = SDL_GetTicks();

        // Update the uniforms
        bool significantChange = synchronizer.syncUniforms(uniforms);

        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplSDL2_NewFrame();
        ImGui::NewFrame();
        drawGui(uniforms, fps);

        oglCtx.useFirstPassProgram();
        oglCtx.useSecondPassProgram(significantChange);

        ImGui::Render();
        ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
        SDL_GL_SwapWindow(window);
    }
}


int main(int argc, char* argv[])
{
    ge::gl::init();

    auto sdlGuard = SDL_Guard();
    auto window   = sdlGuard.createWindow();
    auto context  = sdlGuard.getContext();

    auto imGuiGuard = ImGui_Guard(window, context);

    auto openglContext = OpenGLContext();

    mainloop(window, openglContext);

    return 0;
}

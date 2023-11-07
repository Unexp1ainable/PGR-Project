#include "imgui_stuff.h"
#include <glm/gtc/type_ptr.hpp>
#include <imgui.h>
#include "imgui_impl_opengl3.h"
#include <imgui_impl_sdl2.h>


ImGui_Guard::ImGui_Guard(SDL_Window* window, SDL_GLContext& context)
{
    auto imguiCtx = ImGui::CreateContext();
    ImGuiIO& io   = ImGui::GetIO();
    io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard; // Enable Keyboard Controls
    io.ConfigFlags |= ImGuiConfigFlags_NavEnableGamepad; // Enable Gamepad Controls

    ImGui_ImplSDL2_InitForOpenGL(window, context);
    ImGui_ImplOpenGL3_Init();
}

ImGui_Guard::~ImGui_Guard()
{
    ImGui_ImplOpenGL3_Shutdown();
    ImGui_ImplSDL2_Shutdown();
    ImGui::DestroyContext();
}

void drawGui(UniformStore& store)
{
    ImGui::Begin("Uniforms");
    ImGui::SliderFloat3("lightPosition", glm::value_ptr(store.lightPosition), -10., 10.);
    ImGui::End();
}



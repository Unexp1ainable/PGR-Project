#include "imgui_stuff.h"
#include "imgui_impl_opengl3.h"
#include <glm/gtc/type_ptr.hpp>
#include <imgui.h>
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

void drawGui(UniformStore& store, float fps)
{
    ImGui::Begin("Info");
    ImGui::SeparatorText("Scene");
    ImGui::SliderFloat3("lightPos", glm::value_ptr(store.lightPosition), -100., 100.);
    ImGui::SliderFloat("roughness", &store.roughness, 0., 1.);
    ImGui::SliderFloat("fresnel", &store.fresnel, 0., 1.);
    ImGui::SliderFloat("density", &store.density, 0., 1.);
    ImGui::SliderFloat("n", &store.n, 1., 3.);

    ImGui::SeparatorText("Other");
    ImGui::Text("FPS: %f", fps);
    ImGui::End();
}

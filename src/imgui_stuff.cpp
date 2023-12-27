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
    ImGui::Begin("Properties");
    ImGui::SeparatorText("Scene");
    ImGui::SliderFloat3("lightPos", glm::value_ptr(store.lightPosition), -100., 100.);
    ImGui::InputInt("reflectionBounces", &store.reflectionBounces, 1, 1);
    ImGui::InputInt("refractionBounces", &store.refractionBounces, 1, 1);
    ImGui::InputInt("shadowRays", &store.shadowRays, 1, 1);

    if (ImGui::CollapsingHeader("White material")) {
        ImGui::ColorEdit3("whiteColor", glm::value_ptr(store.whiteColor));
        ImGui::SliderFloat("whiteRoughness", &store.whiteRoughness, 0., 1.);
        ImGui::SliderFloat("whiteTransparency", &store.whiteTransparency, 0., 1.);
        ImGui::SliderFloat("whiteDensity", &store.whiteDensity, 0., 1.);
        ImGui::SliderFloat("whiteN", &store.whiteN, 1., 3.);
    }

    if (ImGui::CollapsingHeader("Black material")) {
        ImGui::SeparatorText("Black material");
        ImGui::ColorEdit3("blackColor", glm::value_ptr(store.blackColor));
        ImGui::SliderFloat("blackRoughness", &store.blackRoughness, 0., 1.);
        ImGui::SliderFloat("blackTransparency", &store.blackTransparency, 0., 1.);
        ImGui::SliderFloat("blackDensity", &store.blackDensity, 0., 1.);
        ImGui::SliderFloat("blackN", &store.blackN, 1., 3.);
    }

    ImGui::SeparatorText("Other");
    ImGui::Text("FPS: %f", fps);
    ImGui::End();
}

#pragma once

#include "common.h"
#include <SDL.h>

class ImGui_Guard {
public:
    ImGui_Guard(SDL_Window* window, SDL_GLContext& context);
    ~ImGui_Guard();

private:
};

void drawGui(UniformStore& store, float fps);

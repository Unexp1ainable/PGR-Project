#pragma once
#include <SDL.h>

class SDL_Guard {
public:
    SDL_Guard();
    ~SDL_Guard();

    SDL_Window* createWindow();

    SDL_Window* getWindow() const { return m_window; }
    SDL_GLContext getContext() const { return m_context; }

private:
    SDL_Window* m_window = nullptr;
    SDL_GLContext m_context = nullptr;
};



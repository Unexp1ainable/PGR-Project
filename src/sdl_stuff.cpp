#include "sdl_stuff.h"


SDL_Guard::SDL_Guard()
{
    SDL_Init(SDL_INIT_VIDEO);
}

SDL_Guard::~SDL_Guard()
{
    SDL_GL_DeleteContext(m_context);
    SDL_DestroyWindow(m_window);
}



SDL_Window* SDL_Guard::createWindow()
{
    m_window = SDL_CreateWindow(
        "PGR_Project", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 1024, 768, SDL_WINDOW_RESIZABLE | SDL_WINDOW_OPENGL | SDL_WINDOW_SHOWN
    );

    uint32_t version = 460; // context version
    uint32_t profile = SDL_GL_CONTEXT_PROFILE_CORE; // context profile
    uint32_t flags   = SDL_GL_CONTEXT_DEBUG_FLAG; // context flags
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, version / 100);
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, (version % 100) / 10);
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, profile);
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, flags);

    m_context = SDL_GL_CreateContext(m_window);
    return m_window;
}


#include<iostream>
#include<vector>
#include<SDL.h>

#include<geGL/geGL.h>
#include<geGL/StaticCalls.h>
using namespace ge::gl;

GLuint createShader(GLenum type,std::string const&src){
  auto id = glCreateShader(type);
  char const*srcs[]={src.data()};
  glShaderSource(id,1,srcs,nullptr);
  glCompileShader(id);
  return id;
}

GLuint createProgram(std::vector<GLuint>const&shaders){
  auto id = glCreateProgram();
  for(auto const&shader:shaders)
    glAttachShader(id,shader);
  glLinkProgram(id);
  return id;
}

int main(int argc,char*argv[]){
  SDL_Init(SDL_INIT_VIDEO);//init. video

  auto window = SDL_CreateWindow("PGR_examples",SDL_WINDOWPOS_CENTERED,SDL_WINDOWPOS_CENTERED,1024,768,SDL_WINDOW_OPENGL);

  uint32_t version = 460;//context version
  uint32_t profile = SDL_GL_CONTEXT_PROFILE_CORE;//context profile
  uint32_t flags   = SDL_GL_CONTEXT_DEBUG_FLAG;//context flags
  SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, version/100 );
  SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION,(version%100)/10);
  SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK ,profile );
  SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS ,flags );

  auto context = SDL_GL_CreateContext(window);

  ge::gl::init();

  GLuint vao;
  glCreateVertexArrays(1,&vao);

  auto vsSrc = R".(
  #version 460
  void main(){
    gl_Position = vec4(gl_VertexID&1,gl_VertexID>>1,0,1);
  }

  ).";

  auto fsSrc = R".(
  #version 460
  layout(location=0)out vec4 fColor;
  void main(){
    fColor = vec4(1,0,0,1);
  }
  ).";

  GLuint prg = createProgram({
      createShader(GL_VERTEX_SHADER,vsSrc),
      createShader(GL_FRAGMENT_SHADER,fsSrc)});

  bool running = true;
  while(running){
    SDL_Event event;
    while(SDL_PollEvent(&event)){
      if(event.type == SDL_QUIT)
        running = false;
    }
    glClearColor(0,0.5,0,1);
    glClear(GL_COLOR_BUFFER_BIT);

    glBindVertexArray(vao);

    glUseProgram(prg);
    glDrawArrays(GL_TRIANGLES,0,3);

    glBindVertexArray(0);
    SDL_GL_SwapWindow(window);
  }

  SDL_GL_DeleteContext(context);
  SDL_DestroyWindow(window);
  return 0;
}

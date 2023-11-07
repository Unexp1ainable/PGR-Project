cmake_minimum_required(VERSION 3.10.1)

project(imgui)
SET(CMAKE_CXX_STANDARD 17)

find_package(SDL2 REQUIRED)
include_directories(${SDL2_INCLUDE_DIRS})

file(GLOB SOURCES "*.cpp")
file(GLOB SOURCES_BACKEND "backends/imgui_impl_sdl2*" "backends/imgui_impl_opengl3*")
file(GLOB SOURCES_MISC "misc/cpp/imgui_stdlib.*")
add_library(${PROJECT_NAME} STATIC ${SOURCES}  ${SOURCES_BACKEND} ${SOURCES_MISC} ${INCLUDES})
target_include_directories(${PROJECT_NAME} PUBLIC
  ${CMAKE_CURRENT_SOURCE_DIR} ${CMAKE_CURRENT_SOURCE_DIR}/backends
)

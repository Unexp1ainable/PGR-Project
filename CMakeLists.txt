cmake_minimum_required(VERSION 3.5)

SET(CMAKE_CXX_STANDARD 20)
SET(CMAKE_C_STANDARD 99)

project(PGR_Project)


file(GLOB_RECURSE SOURCES "src/*.cpp" "src/*.h")


# Set the output directory for the build files
set(CMAKE_BINARY_DIR ${CMAKE_SOURCE_DIR}/build)
set(EXECUTABLE_OUTPUT_PATH ${CMAKE_BINARY_DIR})

option(SDL_SHARED "" OFF)
option(SDL_STATIC "" ON)

add_subdirectory(libs/SDL2-2.28.5)
add_subdirectory(libs/glm)
add_subdirectory(libs/geGL)

# TODO add custom command that embeds shaders into the code


add_executable(${PROJECT_NAME} ${SOURCES})

target_link_libraries(${PROJECT_NAME} SDL2-static geGL::geGL glm)

# suppress warnings from geGl >:(
set_target_properties(geGL PROPERTIES INTERFACE_SYSTEM_INCLUDE_DIRECTORIES $<TARGET_PROPERTY:geGL,INTERFACE_INCLUDE_DIRECTORIES>)

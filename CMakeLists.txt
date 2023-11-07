cmake_minimum_required(VERSION 3.5)
project(PGR_Project)

SET(CMAKE_CXX_STANDARD 20)
SET(CMAKE_C_STANDARD 99)

# Set the output directory for the build files
set(CMAKE_BINARY_DIR ${CMAKE_SOURCE_DIR}/build)
set(EXECUTABLE_OUTPUT_PATH ${CMAKE_BINARY_DIR})

option(SDL_SHARED "" OFF)
option(SDL_STATIC "" ON)

add_subdirectory(libs/SDL2-2.28.5)
add_subdirectory(libs/glm)
add_subdirectory(libs/geGL)
add_subdirectory(libs/imgui-1.89.9)

file(GLOB SHADER_PATHS "src/shaders/*")

# Run stringify.cmake when anything in folder src/shaders changes
foreach(SOURCE_PATH ${SHADER_PATHS})
    get_filename_component(FILE_NAME ${SOURCE_PATH} NAME_WE)
    set(TARGET_PATH ${CMAKE_SOURCE_DIR}/src/generated/shaders/${FILE_NAME}.h)
    list(APPEND SHADER_FILES ${TARGET_PATH})

    add_custom_command(
        OUTPUT ${TARGET_PATH}
        COMMAND ${CMAKE_COMMAND} -DTARGET=${TARGET_PATH} -DSOURCE=${SOURCE_PATH} -P ${CMAKE_SOURCE_DIR}/stringify.cmake
        DEPENDS ${SOURCE_PATH}
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        VERBATIM
    )
endforeach()

file(GLOB_RECURSE SOURCES "src/*.cpp" "src/*.h")
add_executable(${PROJECT_NAME} ${SOURCES} ${SHADER_FILES})

target_link_libraries(${PROJECT_NAME} SDL2-static geGL::geGL glm imgui)

# suppress warnings from geGl >:(
set_target_properties(geGL PROPERTIES INTERFACE_SYSTEM_INCLUDE_DIRECTORIES $<TARGET_PROPERTY:geGL,INTERFACE_INCLUDE_DIRECTORIES>)
# and also glm
set_target_properties(glm PROPERTIES INTERFACE_SYSTEM_INCLUDE_DIRECTORIES $<TARGET_PROPERTY:glm,INTERFACE_INCLUDE_DIRECTORIES>)

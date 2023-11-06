# # Define a function that takes a list of files and appends "first" in front of them
file(GLOB SHADERS "${CMAKE_SOURCE_DIR}/src/shaders/*")

message(${SOURCE})
message(${TARGET})
get_filename_component(VAR_NAME ${SOURCE} NAME_WE)
string(TOUPPER ${VAR_NAME} VAR_NAME)
file(READ ${SOURCE} contents)
file(WRITE ${TARGET} "constexpr const char* ${VAR_NAME} = R\"(${contents})\";")

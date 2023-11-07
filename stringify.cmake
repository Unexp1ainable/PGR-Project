# convert file into header file where the contents of file are stored as char*
file(GLOB SHADERS "${CMAKE_SOURCE_DIR}/src/shaders/*")

message(${SOURCE})
message(${TARGET})
get_filename_component(VAR_NAME ${SOURCE} NAME_WE)
string(TOUPPER ${VAR_NAME} VAR_NAME)
file(READ ${SOURCE} contents)
file(WRITE ${TARGET} "constexpr const char* ${VAR_NAME}_SOURCE = R\"(${contents})\";")

#version 460
layout(location=0)in int id;

void main()
{
    gl_Position = vec4(gl_VertexID & 1, gl_VertexID >> 1, 0, 1);
}

#version 460
void main()
{
    gl_Position = vec4(gl_VertexID & 1, gl_VertexID >> 1, 0, 1);
}

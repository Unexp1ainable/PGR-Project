#version 460

uniform sampler2D myTexture; // Your texture sampler

out vec4 FragColor;

void main() {
    vec2 texCoord = gl_FragCoord.xy / textureSize(myTexture,0);
    FragColor = texture(myTexture, texCoord);
    // FragColor = vec4(ts.x/2., 0, 0.0, 1.0);
}

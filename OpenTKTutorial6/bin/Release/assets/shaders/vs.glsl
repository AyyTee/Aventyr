#version 330

in  vec3 vPosition;
in  vec3 vColor;
out vec4 color;
uniform mat4 modelMatrix;

void
main()
{
    gl_Position = modelMatrix * vec4(vPosition, 1.0);
    color = vec4( vColor, 1.0);
}
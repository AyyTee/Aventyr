#version 330

in  vec3 vPosition;
in vec2 texcoord;
out vec2 f_texcoord;
uniform mat4 modelMatrix;

void
main()
{
	gl_Position = modelMatrix * vec4(vPosition, 1.0);
    f_texcoord = vec2(texcoord.x, texcoord.y);
}
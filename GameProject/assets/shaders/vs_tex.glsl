#version 130

in  vec3 vPosition;
in vec2 texcoord;
out vec2 f_texcoord;
uniform mat4 modelMatrix;
uniform mat4 UVMatrix;

void
main()
{
	gl_Position = modelMatrix * vec4(vPosition, 1.0);
	vec4 v = UVMatrix * vec4(texcoord.x, texcoord.y, 0.0, 1.0);
    f_texcoord = vec2(v.x, v.y);
}
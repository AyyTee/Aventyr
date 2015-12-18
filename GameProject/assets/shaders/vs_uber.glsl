#version 330

in vec3 vPosition;
in vec3 vColor;
in vec2 texcoord;
out vec4 f_color;
out vec2 f_texcoord;
uniform mat4 modelMatrix;
uniform mat4 UVMatrix;
uniform mat4 viewMatrix;

void
main()
{
	vec4 v = UVMatrix * vec4(texcoord.x, texcoord.y, 0.0, 1.0);
	f_texcoord = v.xy;
	f_color = vec4(vColor, 1.0);
	gl_Position = viewMatrix * modelMatrix * vec4(vPosition, 1.0);
}
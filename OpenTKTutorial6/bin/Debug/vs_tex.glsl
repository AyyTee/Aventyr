#version 330

in  vec3 vPosition;
in vec2 texcoord;
out vec2 f_texcoord;

uniform float timeDelta;
uniform vec3 speed;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;

void
main()
{
	vec4 vPosGlobal = modelMatrix * vec4(vPosition, 1.0);
	vec4 vPosDelta = vPosGlobal + vec4(speed, 0.0) * timeDelta;
    gl_Position = viewMatrix * vPosDelta;
	//gl_Position = viewMatrix * modelMatrix * vec4(vPosition, 1.0);
    f_texcoord = texcoord;
}
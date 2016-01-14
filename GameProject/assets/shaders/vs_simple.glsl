#version 130

in vec3 vPosition;
//uniform float offset[10];

void
main()
{
	gl_Position = vec4(vPosition, 1.0);
	float a = gl_InstanceID;
	gl_Position.x -= a/20;//offset[gl_InstanceID];
	//gl_Position = vec4(offset[gl_InstanceID], 0, 0, 1.0);
}
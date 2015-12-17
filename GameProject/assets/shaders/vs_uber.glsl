#version 330

in vec3 vPosition;
in vec3 vColor;
in vec2 texcoord;
uniform float offset[10];
/*out vec2 v_texcoord;
out vec4 v_color;*/
out vec4 f_color;
out vec2 f_texcoord;
uniform mat4 modelMatrix;
uniform mat4 UVMatrix;
uniform mat4 viewMatrix;
//flat out int InstanceID;

void
main()
{
	vec4 v = UVMatrix * vec4(texcoord.x, texcoord.y, 0.0, 1.0);
	/*v_texcoord = vec2(v.x, v.y);
	v_color = vec4(vColor, 1.0);*/
	f_texcoord = v.xy;
	f_color = vec4(vColor, 1.0);
	vec4 position = viewMatrix * modelMatrix * vec4(vPosition.x + offset[0], vPosition.y, vPosition.z, 1.0);
	gl_Position = position;
	//InstanceID = gl_InstanceID;
}
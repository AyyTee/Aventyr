#version 330
layout(triangles) in;
layout(triangle_strip, max_vertices=3) out;

in vec4 v_color [];
in vec2 v_texcoord [];
out vec4 f_color;
out vec2 f_texcoord;

void main()
{	
	for (int i = 0; i < v_color.length(); i++)
	{
		f_color = v_color[i];
	}
	for (int i = 0; i < v_texcoord.length(); i++)
	{
		f_texcoord = v_texcoord[i];
	}
	for(int j = 0; j < 3; j++)
	{
		gl_Position = gl_in[j].gl_Position;
		EmitVertex();
	}
	EndPrimitive();
}  
#version 330

in vec4 f_color;
in vec2 f_texcoord;
out vec4 outputColor;

uniform int isTextured;
//uniform int isDithered;
uniform sampler2D maintexture;
//uniform sampler2D bayer_matrix;

void
main()
{
	if (isTextured == 1)
	{
		vec2 flipped_texcoord = vec2(f_texcoord.x, f_texcoord.y);
		vec4 color = texture(maintexture, flipped_texcoord);
		color.x *= f_color.x;
		color.y *= f_color.y;
		color.z *= f_color.z;
		color.w *= f_color.w;
		outputColor = color;
	}
	else
	{
		outputColor = f_color;
	}
}
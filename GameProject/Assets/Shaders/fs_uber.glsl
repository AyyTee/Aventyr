#version 330

in vec4 f_color;
in vec2 f_texcoord;
out vec4 outputColor;

uniform int isTextured;
uniform sampler2D maintexture;

void
main()
{
	if (isTextured == 1)
	{
		vec2 flipped_texcoord = vec2(f_texcoord.x, f_texcoord.y);
		outputColor = texture(maintexture, flipped_texcoord);
	}
	else
	{
		outputColor = f_color;
	}
}
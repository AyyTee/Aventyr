#version 330

in vec2 f_texcoord;
out vec4 outputColor;

uniform sampler2D maintexture;

void
main()
{
	vec2 flipped_texcoord = vec2(f_texcoord.x, 1.0 - f_texcoord.y);
	if (flipped_texcoord.x > 0.5)
	{
		outputColor = texture(maintexture, flipped_texcoord);
	}
	else
	{
		outputColor = vec4(0, 0, 0, 0);
	}
}
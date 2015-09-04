#version 330

in vec2 f_texcoord;
out vec4 outputColor;

uniform sampler2D maintexture;
uniform vec2 cullLine0;
uniform vec2 cullLine1;

void
main()
{
	if (cullLine0 != cullLine1)
	{
		if (0 < (cullLine1.x - cullLine0.x) * (gl_FragCoord.y - cullLine0.y) - (cullLine1.y - cullLine0.y) * (gl_FragCoord.x - cullLine0.x))
		{
			discard;
			return;
		}
	}
	vec2 flipped_texcoord = vec2(f_texcoord.x, 1.0 - f_texcoord.y);
	outputColor = texture(maintexture, flipped_texcoord);
}
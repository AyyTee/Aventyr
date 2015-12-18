#version 330

in vec4 f_color;
in vec2 f_texcoord;
out vec4 outputColor;

uniform int isTextured;
uniform sampler2D maintexture;
uniform float cutLines[32]; //max number of portals that can clip an object is equal to array length/4
uniform int cutLinesLength;
//flat in int InstanceID;

void
main()
{
	for (int i = 0; i < min(cutLinesLength, cutLines.length()); i += 4)
	{
		vec2 v0 = vec2(cutLines[i], cutLines[i+1]);
		vec2 v1 = vec2(cutLines[i+2], cutLines[i+3]);
		//debug color if the vertices lie exactly on top of eachother
		/*if (v0 == v1 && i == 0)
		{
			outputColor = vec4(1, 1, 0, 1);
			return;
		}*/
		
		if (0 < (v1.x - v0.x) * (gl_FragCoord.y - v0.y) - (v1.y - v0.y) * (gl_FragCoord.x - v0.x))
		{
			discard;
			return;
		}
	}
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
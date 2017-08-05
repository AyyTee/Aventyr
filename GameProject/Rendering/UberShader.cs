using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public static class UberShader
    {
        public static string FragmentShader => @"
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
		//color.x = (int(gl_FragCoord.x) % 100) / 100;//texture(bayer_matrix, gl_FragCoord.xy).x;
		outputColor = color;
	}
	else
	{
		outputColor = f_color;
	}
}
";
        public static string VertexShader => @"
#version 330

in vec3 vPosition;
in vec4 vColor;
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
	f_color = vColor;
	gl_Position = viewMatrix * modelMatrix * vec4(vPosition, 1.0);
}
";
    }
}

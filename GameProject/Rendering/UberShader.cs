using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public static class UberShader
    {
        const string Version = "330";

        const string FragColor = nameof(FragColor);
        const string FragUvCoord = nameof(FragUvCoord);
        const string OutputColor = nameof(OutputColor);

        public const string IsTextured = nameof(IsTextured);
        public const string IsDithered = nameof(IsDithered);
        public const string MainTexture = nameof(MainTexture);

        public const string VertPosition = nameof(VertPosition);
        public const string VertColor = nameof(VertColor);
        public const string VertUvCoord = nameof(VertUvCoord);
        public const string ModelMatrix = nameof(ModelMatrix);
        public const string UvMatrix = nameof(UvMatrix);
        public const string ViewMatrix = nameof(ViewMatrix);

        public static string GetFragmentShader()
        {
            var iterations = 3;
            var bayerMatrix = MathEx.BayerMatrix(iterations);
            var matrixWidth = 1 << iterations;
            var elementCount = matrixWidth * matrixWidth;

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < elementCount; i++)
            {
                var value = bayerMatrix[i / matrixWidth, i % matrixWidth] / (float)elementCount;
                stringBuilder.Append((i == 0 ? "" : ", ") + value);
            }

            return $@"
#version {Version}

in vec4 {FragColor};
in vec2 {FragUvCoord};
out vec4 {OutputColor};

uniform int {IsTextured};
uniform int {IsDithered};
uniform sampler2D {MainTexture};

const float bayerMatrix[{elementCount}] = float[] ({stringBuilder.ToString()});

void
main()
{{
	if ({IsTextured} == 1)
	{{
		vec2 flipped_texcoord = vec2({FragUvCoord}.x, {FragUvCoord}.y);
		vec4 color = texture({MainTexture}, flipped_texcoord) * {FragColor};
		{OutputColor} = color;
	}}
	else
	{{
		{OutputColor} = {FragColor};
	}}

    if ({IsDithered} == 1)
    {{
        {OutputColor}.w += bayerMatrix[(int(gl_FragCoord.y) % {matrixWidth}) * {matrixWidth} + int(gl_FragCoord.x) % {matrixWidth}];
        {OutputColor}.w = {OutputColor}.w > 1 ? 1 : 0;
    }}

    /* This condition will never be met but it prevents attributes and uniforms from getting 
     * optimized away when we're trying to debug.*/
    if ({IsTextured} == 2)
    {{
        {OutputColor}.x = {FragUvCoord}.x;
    }}
}}
";
        }

        public static string GetVertexShader()
        {
            return $@"
#version {Version}

in vec3 {VertPosition};
in vec4 {VertColor};
in vec2 {VertUvCoord};
out vec4 {FragColor};
out vec2 {FragUvCoord};
uniform mat4 {ModelMatrix};
uniform mat4 {UvMatrix};
uniform mat4 {ViewMatrix};

void
main()
{{
	vec4 v = {UvMatrix} * vec4({VertUvCoord}.x, {VertUvCoord}.y, 0.0, 1.0);
	{FragUvCoord} = v.xy;
	{FragColor} = {VertColor};
	gl_Position = {ViewMatrix} * {ModelMatrix} * vec4({VertPosition}, 1.0);
}}
";
        }
    }
}

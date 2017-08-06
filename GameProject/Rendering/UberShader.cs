using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public class UberShader
    {
        const string Version = "330";

        const string FragColor = nameof(FragColor);
        const string FragUvCoord = nameof(FragUvCoord);
        const string OutputColor = nameof(OutputColor);

        public const string IsTextured = nameof(IsTextured);
        public const string IsDithered = nameof(IsDithered);
        public const string MainTexture = nameof(MainTexture);
        public const string BayerMatrix = nameof(BayerMatrix);

        public const string VertPosition = nameof(VertPosition);
        public const string VertColor = nameof(VertColor);
        public const string VertUvCoord = nameof(VertUvCoord);
        public const string ModelMatrix = nameof(ModelMatrix);
        public const string UvMatrix = nameof(UvMatrix);
        public const string ViewMatrix = nameof(ViewMatrix);

        readonly int _bayerMatrixSize;

        public string FragmentShader => $@"
#version {Version}

in vec4 {FragColor};
in vec2 {FragUvCoord};
out vec4 {OutputColor};

uniform int {IsTextured};
uniform int {IsDithered};
uniform sampler2D {MainTexture};
uniform sampler2D {BayerMatrix};

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
        {OutputColor}.w += texture({BayerMatrix}, gl_FragCoord.xy / {_bayerMatrixSize}).x;
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

        public string VertexShader => $@"
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

        public UberShader(int bayerMatrixSize)
        {
            _bayerMatrixSize = bayerMatrixSize;
        }
    }
}

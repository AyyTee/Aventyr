using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game.Common;
using OpenTK.Graphics.OpenGL;

namespace Game.Rendering
{
    public class Shader
    {
        public readonly int ProgramId = -1;
        public readonly int VShaderId = -1;
        public readonly int FShaderId = -1;

        public Dictionary<string, AttributeInfo> Attributes { get; } = new Dictionary<string, AttributeInfo>();
        public Dictionary<string, UniformInfo> Uniforms { get; } = new Dictionary<string, UniformInfo>();
        public Dictionary<string, uint> Buffers { get; } = new Dictionary<string, uint>();

        public int Vao { get; private set; }

		public Shader(string vshader, string fshader)
		{
			ProgramId = GL.CreateProgram();

            LoadShader(vshader, ShaderType.VertexShader, out VShaderId);
            LoadShader(fshader, ShaderType.FragmentShader, out FShaderId);

            Link();
			GenBuffers();
            DebugEx.GlAssert();
        }

        void LoadShader(string code, ShaderType type, out int address)
        {
            address = GL.CreateShader(type);
            GL.ShaderSource(address, code);
            GL.CompileShader(address);
            GL.AttachShader(ProgramId, address);
            var shaderInfo = GL.GetShaderInfoLog(address);
            Console.WriteLine(shaderInfo);
            DebugEx.Assert(shaderInfo == "");
        }

        private void Link()
        {
            GL.LinkProgram(ProgramId);

            Console.WriteLine(GL.GetProgramInfoLog(ProgramId));

            GL.GetProgram(ProgramId, GetProgramParameterName.ActiveAttributes, out int attributeCount);
            GL.GetProgram(ProgramId, GetProgramParameterName.ActiveUniforms, out int uniformCount);

            Vao = GL.GenVertexArray();

            for (int i = 0; i < attributeCount; i++)
            {
                var info = new AttributeInfo();

                var name = new StringBuilder();

                GL.GetActiveAttrib(ProgramId, i, 256, out int length, out info.Size, out info.Type, name);

                info.Name = name.ToString();
                info.Address = GL.GetAttribLocation(ProgramId, info.Name);
                Attributes.Add(name.ToString(), info);
            }

            for (int i = 0; i < uniformCount; i++)
            {
                var info = new UniformInfo();

                var name = new StringBuilder();

                GL.GetActiveUniform(ProgramId, i, 256, out int length, out info.Size, out info.Type, name);

                info.Name = name.ToString();
                Uniforms.Add(name.ToString(), info);
                info.Address = GL.GetUniformLocation(ProgramId, info.Name);
            }
        }

        public void GenBuffers()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                GL.GenBuffers(1, out uint buffer);
                Buffers.Add(Attributes.Values.ElementAt(i).Name, buffer);
            }

            for (int i = 0; i < Uniforms.Count; i++)
            {
                GL.GenBuffers(1, out uint buffer);
                Buffers.Add(Uniforms.Values.ElementAt(i).Name, buffer);
            }
        }

        public void EnableVertexAttribArrays()
        {
            GL.BindVertexArray(Vao);
            for (int i = 0; i < Attributes.Count; i++)
            {
                GL.EnableVertexAttribArray(Attributes.Values.ElementAt(i).Address);
            }
        }

        public void DisableVertexAttribArrays()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                GL.DisableVertexAttribArray(Attributes.Values.ElementAt(i).Address);
            }
        }

        public class UniformInfo
        {
            public string Name = "";
            public int Address = -1;
            public int Size;
            public ActiveUniformType Type;
        }

        public class AttributeInfo
        {
            public string Name = "";
            public int Address = -1;
            public int Size;
            public ActiveAttribType Type;
        }
    }
}

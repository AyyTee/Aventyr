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
        public int ProgramId = -1;
        public int VShaderId = -1;
        public int GShaderId = -1;
        public int FShaderId = -1;
        public int AttributeCount;
        public int UniformCount;

        public Dictionary<string, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>();
        public Dictionary<string, UniformInfo> Uniforms = new Dictionary<string, UniformInfo>();
        public Dictionary<string, uint> Buffers = new Dictionary<string, uint>();

        public int Vao { get; private set; }

        public Shader()
        {
            ProgramId = GL.CreateProgram();
        }

		public Shader(string vshader, string fshader, bool fromFile = false)
		{
			ProgramId = GL.CreateProgram();

			if (fromFile)
			{
				LoadShaderFromFile(vshader, ShaderType.VertexShader);
				LoadShaderFromFile(fshader, ShaderType.FragmentShader);
			}
			else
			{
				LoadShaderFromString(vshader, ShaderType.VertexShader);
				LoadShaderFromString(fshader, ShaderType.FragmentShader);
			}

			Link();
			GenBuffers();
		}

		public Shader(string vshader, string gshader, string fshader, bool fromFile = false)
		{
			ProgramId = GL.CreateProgram();

			if (fromFile)
			{
				LoadShaderFromFile(vshader, ShaderType.VertexShader);
				LoadShaderFromFile(gshader, ShaderType.GeometryShader);
				LoadShaderFromFile(fshader, ShaderType.FragmentShader);
			}
			else
			{
				LoadShaderFromString(vshader, ShaderType.VertexShader);
				LoadShaderFromString(gshader, ShaderType.GeometryShader);
				LoadShaderFromString(fshader, ShaderType.FragmentShader);
			}

			Link();
			GenBuffers();
		}

        void LoadShader(string code, ShaderType type, out int address)
        {
            address = GL.CreateShader(type);
            GL.ShaderSource(address, code);
            GL.CompileShader(address);
            GL.AttachShader(ProgramId, address);
            var glError = GL.GetError();
            DebugEx.Assert(glError == ErrorCode.NoError);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        public void LoadShaderFromString(string code, ShaderType type)
        {
            switch (type)
            {
                case ShaderType.VertexShader:
                    LoadShader(code, type, out VShaderId);
                    break;
                case ShaderType.GeometryShader:
                    LoadShader(code, type, out GShaderId);
                    break;
                case ShaderType.FragmentShader:
                    LoadShader(code, type, out FShaderId);
                    break;
            }
        }

        public void LoadShaderFromFile(string filename, ShaderType type)
        {
            using (var sr = new StreamReader(filename))
            {
                switch (type)
                {
                    case ShaderType.VertexShader:
                        LoadShader(sr.ReadToEnd(), type, out VShaderId);
                        break;
                    case ShaderType.GeometryShader:
                        LoadShader(sr.ReadToEnd(), type, out GShaderId);
                        break;
                    case ShaderType.FragmentShader:
                        LoadShader(sr.ReadToEnd(), type, out FShaderId);
                        break;
                }
            }
        }

        private void Link()
        {
            GL.LinkProgram(ProgramId);

            Console.WriteLine(GL.GetProgramInfoLog(ProgramId));

            GL.GetProgram(ProgramId, GetProgramParameterName.ActiveAttributes, out AttributeCount);
            GL.GetProgram(ProgramId, GetProgramParameterName.ActiveUniforms, out UniformCount);

            Vao = GL.GenVertexArray();

            for (int i = 0; i < AttributeCount; i++)
            {
                var info = new AttributeInfo();
                int length;

                var name = new StringBuilder();

                GL.GetActiveAttrib(ProgramId, i, 256, out length, out info.Size, out info.Type, name);

                info.Name = name.ToString();
                info.Address = GL.GetAttribLocation(ProgramId, info.Name);
                Attributes.Add(name.ToString(), info);
            }

            for (int i = 0; i < UniformCount; i++)
            {
                var info = new UniformInfo();
                int length;

                var name = new StringBuilder();

                GL.GetActiveUniform(ProgramId, i, 256, out length, out info.Size, out info.Type, name);

                info.Name = name.ToString();
                Uniforms.Add(name.ToString(), info);
                info.Address = GL.GetUniformLocation(ProgramId, info.Name);
            }
        }

        public void GenBuffers()
        {
            for (int i = 0; i < Attributes.Count; i++)
            {
                uint buffer;
                GL.GenBuffers(1, out buffer);

                Buffers.Add(Attributes.Values.ElementAt(i).Name, buffer);
            }

            for (int i = 0; i < Uniforms.Count; i++)
            {
                uint buffer;
                GL.GenBuffers(1, out buffer);

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

        public int GetAttribute(string name)
        {
            if (Attributes.ContainsKey(name))
            {
                return Attributes[name].Address;
            }
            else
            {
                return -1;
            }
        }

        public int GetUniform(string name)
        {
            if (Uniforms.ContainsKey(name))
            {
                return Uniforms[name].Address;
            }
            else
            {
                return -1;
            }
        }

        public uint GetBuffer(string name)
        {
            if (Buffers.ContainsKey(name))
            {
                return Buffers[name];
            }
            else
            {
                return 0;
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

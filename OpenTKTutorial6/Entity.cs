using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// An object that exists within the world space and can be drawn
    /// </summary>
    class Entity
    {
        public Transform Transform { get; set; }
        public Transform Velocity { get; set; }
        public List<Model> Models { get; set; }
        public Entity(Vector3 Position)
        {
            Transform = new Transform(Position);
            Velocity = new Transform();
            Models = new List<Model>();
        }
        public void StepUpdate()
        {
            foreach (Model v in Models)
            {
                v.Transform.GetMatrix();
            }
        }
        public void BufferModels()
        {

        }
        public void Render(Matrix4 viewMatrix, float timeDelta, ref int indiceat)
        {
            Transform.GetMatrix();
            foreach (Model v in Models)
            {
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);
                Matrix4 modelMatrix = v.Transform.TransformMatrix * Transform.TransformMatrix;
                GL.UniformMatrix4(v.Shader.GetUniform("modelMatrix"), false, ref modelMatrix);
                GL.UniformMatrix4(v.Shader.GetUniform("viewMatrix"), false, ref viewMatrix);

                GL.Uniform1(v.Shader.GetUniform("timeDelta"), timeDelta);
                //GL.Uniform3(shaders[activeShader].GetUniform("speed"), ref v.Speed);
                if (v.Shader.GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(v.Shader.GetAttribute("maintexture"), v.TextureID);
                }

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }
        }
        public Transform GetRenderTransform(float deltaTime)
        {
            return Transform.Lerp(Transform, Velocity, deltaTime);
        }
    }
}
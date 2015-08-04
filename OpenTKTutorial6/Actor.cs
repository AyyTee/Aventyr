using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
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

        }
        public void RenderUpdate()
        {
            
        }
        public Transform GetRenderTransform(float deltaTime)
        {
            return Transform + Velocity * deltaTime;
        }
    }
}
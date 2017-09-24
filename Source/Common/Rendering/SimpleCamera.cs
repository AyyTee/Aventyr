using Game.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    /// <summary>
    /// Simple ICamera2 implementation for unit testing.
    /// </summary>
    [DataContract]
    public class SimpleCamera2 : ICamera2
    {
        [DataMember]
        public float Aspect { get; set; } = 1;
        [DataMember]
        public double Fov { get; set; } = Math.PI / 4;
        [DataMember]
        public Vector2 ViewOffset { get; set; }
        [DataMember]
        public Transform2 WorldTransform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 WorldVelocity { get; set; } = Transform2.CreateVelocity();
        [DataMember]
        public bool IsOrtho { get; set; } = true;

        public SimpleCamera2()
        {
        }

        public SimpleCamera2(ICamera2 camera)
        {
            Aspect = camera.Aspect;
            Fov = camera.Fov;
            ViewOffset = camera.ViewOffset;
            WorldTransform = camera.WorldTransform;
            WorldVelocity = camera.WorldVelocity;
            IsOrtho = camera.IsOrtho;
        }
    }
}

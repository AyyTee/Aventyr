using Game.Common;
using Game.Rendering;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    [DataContract]
    public class GridCamera : ICamera2
    {
        [DataMember]
        public float Aspect { get; set; }

        public Vector2 ViewOffset => new Vector2();

        public double Fov => Math.PI / 4;

        [DataMember]
        public Transform2 WorldTransform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 WorldVelocity { get; set; } = Transform2.CreateVelocity();

        [DataMember]
        public bool IsOrtho { get; set; } = true;

        public GridCamera(Transform2 worldTransform, float aspect)
        {
            DebugEx.Assert(aspect != 0);
            WorldTransform = worldTransform;
            Aspect = aspect;
        }
    }
}

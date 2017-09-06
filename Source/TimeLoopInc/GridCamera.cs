using Game.Common;
using Game.Rendering;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class GridCamera : ICamera2
    {
        public float Aspect { get; set; }

        public Vector2 ViewOffset => new Vector2();

        public float ZNear => -10000f;
        public float ZFar => 10000f;
        public double Fov => Math.PI / 4;

        public Transform2 WorldTransform { get; set; } = new Transform2();
        public Transform2 WorldVelocity { get; set; } = Transform2.CreateVelocity();

        public GridCamera(Transform2 worldTransform, float aspect)
        {
            DebugEx.Assert(aspect != 0);
            WorldTransform = worldTransform;
            Aspect = aspect;
        }
    }
}

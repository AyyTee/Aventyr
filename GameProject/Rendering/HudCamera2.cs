using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using OpenTK;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Game.Rendering
{
    [DataContract]
    public class HudCamera2 : ICamera2
    {
        public float Aspect => (float)CanvasSize.XRatio;

        public Vector2 ViewOffset => new Vector2(1f, 1f);

        public float ZNear => -10000f;
        public float ZFar => 10000f;
        public double Fov => Math.PI / 4;

        public Vector2i CanvasSize { get; set; }

        public Transform2 WorldTransform => new Transform2(new Vector2(), 0, CanvasSize.Y);
        public Transform2 WorldVelocity => Transform2.CreateVelocity();

        public HudCamera2(Vector2i canvasSize)
        {
            CanvasSize = canvasSize;
        }
    }
}

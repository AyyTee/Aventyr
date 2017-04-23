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
        public float Aspect => CanvasSize.X / (float)CanvasSize.Y;

        public Vector2 ViewOffset => new Vector2();

        public float ZNear => -10000f;
        public float ZFar => 10000f;
        public double Fov => Math.PI / 4;

        public Vector2i CanvasSize { get; set; }

        public HudCamera2(Vector2i canvasSize)
        {
            CanvasSize = canvasSize;
        }

        public Transform2 GetWorldTransform(bool ignorePortals = false) => new Transform2(new Vector2(), CanvasSize.Y);

        public Transform2 GetWorldVelocity(bool ignorePortals = false) => Transform2.CreateVelocity();
    }
}

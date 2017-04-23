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
    public class SimpleCamera2 : ICamera2
    {
        [DataMember] float _aspect = 1;
        /// <summary>
        /// The aspect ratio for the camera.  
        /// Window.width / Window.height will give an aspect that matches the window.
        /// </summary>
        public float Aspect
        {
            get { return _aspect; }
            set
            {
                Debug.Assert(value > 0);
                _aspect = value;
            }
        }

        public Vector2 ViewOffset => new Vector2();

        public float ZNear => -10000f;
        public float ZFar => 10000f;
        public double Fov => Math.PI / 4;

        public Transform2 GetWorldTransform(bool ignorePortals = false) => new Transform2();

        public Transform2 GetWorldVelocity(bool ignorePortals = false) => Transform2.CreateVelocity();
    }
}

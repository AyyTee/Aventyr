using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Game.Common;
using Game.Portals;
using Game.Serialization;
using OpenTK;

namespace Game.Rendering
{
    [DataContract]
    public class Camera2 : SceneNode, ICamera2, IPortalable
    {
        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        [DataMember]
        private float _aspect = 1;
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
        [DataMember]
        public Vector2 ViewOffset { get; set; }
        public float ZNear => -10000f;
        public float ZFar => 10000f;
        public double Fov => Math.PI / 4;

        [DataMember]
        public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }

        #region Constructors
        public Camera2(Scene scene)
            : this(scene, new Transform2(), 1)
        {
        }

        public Camera2(Scene scene, Vector2 position, float aspectRatio)
            : this(scene, new Transform2(position), aspectRatio)
        {
        }

        public Camera2(Scene scene, Transform2 transform, float aspectRatio)
            : base(scene)
        {
            SetTransform(transform);
            Aspect = aspectRatio;
        }
        #endregion

        public override IDeepClone ShallowClone()
        {
            Camera2 clone = new Camera2(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected void ShallowClone(Camera2 destination)
        {
            base.ShallowClone(destination);
            destination.Aspect = Aspect;
            destination.ViewOffset = ViewOffset;
        }

        public sealed override void SetTransform(Transform2 transform)
        {
            Transform = transform.ShallowClone();
            base.SetTransform(transform);
        }

        public override Transform2 GetTransform()
        {
            return Transform.ShallowClone();
        }

        public override Transform2 GetVelocity()
        {
            return Velocity.ShallowClone();
        }

        public override void SetVelocity(Transform2 velocity)
        {
            Velocity = velocity.ShallowClone();
            base.SetVelocity(velocity);
        }

        public List<IPortal> GetPortalChildren()
        {
            return Children.OfType<IPortal>().ToList();
        }
    }
}

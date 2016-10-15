using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class Camera2 : SceneNode, ICamera2, IPortalable
    {
        [DataMember]
        public Transform2 Transform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 Velocity { get; set; } = Transform2.CreateVelocity();
        [DataMember]
        public float _aspect = 1;
        public float Aspect
        {
            get { return _aspect; }
            set
            {
                Debug.Assert(value >= 0);
                _aspect = value;
            }
        }
        /// <summary>View offset in clip space coordinates [-1,1].</summary>
        [DataMember]
        public Vector2 ViewOffset { get; set; }
        public float ZNear { get { return -10000f; } }
        public float ZFar { get { return 10000f; } }
        public double Fov { get { return Math.PI / 4; } }
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
            ViewOffset = new Vector2();
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

        public Matrix4 GetViewMatrix(bool isOrtho = true)
        {
            return CameraExt.GetViewMatrix(this, isOrtho);
        }

        public override void SetTransform(Transform2 transform)
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

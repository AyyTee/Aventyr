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
        Transform2 _transform = new Transform2();
        [DataMember]
        Transform2 _velocity = new Transform2();
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

        public void SetTransform(Transform2 transform)
        {
            _transform = transform.ShallowClone();
        }

        public override Transform2 GetTransform()
        {
            return _transform.ShallowClone();
        }

        public override Transform2 GetVelocity()
        {
            return _velocity.ShallowClone();
        }

        public void SetVelocity(Transform2 transform)
        {
            _velocity = transform.ShallowClone();
        }
    }
}

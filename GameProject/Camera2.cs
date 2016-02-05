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
    public class Camera2 : SceneNodePlaceable
    {
        [DataMember]
        public float _aspect = 1;
        public float Aspect
        {
            get
            {
                return _aspect;
            }
            set
            {
                Debug.Assert(value >= 0);
                _aspect = value;
            }
        }
        /// <summary>Position used for casting line of sight rays for portals</summary>
        [DataMember]
        public Vector2 Viewpos { get; set; }
        [DataMember]
        public float ZNear { get; set; }
        [DataMember]
        public float ZFar { get; set; }

        public const double fov = Math.PI/2;

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
            ZNear = -10000f;
            ZFar = 10000f;
            Viewpos = new Vector2();
        }
        #endregion

        public override SceneNode Clone(Scene scene)
        {
            Camera2 clone = new Camera2(scene);
            Clone(clone);
            return clone;
        }

        protected override void Clone(SceneNode destination)
        {
            base.Clone(destination);
            Camera2 destinationCast = (Camera2)destination;
            destinationCast.Aspect = Aspect;
            //destinationCast.Zoom = Zoom;
            destinationCast.ZNear = ZNear;
            destinationCast.ZFar = ZFar;
            destinationCast.Viewpos = Viewpos;
        }

        public Vector2 GetWorldViewpos()
        {
            Transform2 transform = GetWorldTransform();
            return transform.Position + Viewpos;
        }

        /// <summary>
        /// Create a view matrix for this Camera
        /// </summary>
        /// <returns>A view matrix to look in the camera's direction</returns>
        public Matrix4 GetViewMatrix(bool isOrtho = true)
        {
            Transform2 transform = GetWorldTransform();
            Matrix4 m = Matrix4.CreateRotationZ(transform.Rotation);
            Vector3 lookat = Vector3.Transform(new Vector3(0, 0, -1), m);
            Vector3 eye;
            Matrix4 perspective;
            if (isOrtho)
            {
                perspective = Matrix4.CreateOrthographic(transform.Scale.X * Aspect, transform.Scale.Y, ZNear, ZFar);
                eye = new Vector3(transform.Position) + new Vector3(0, 0, 5000);
            }
            else
            {
                perspective = Matrix4.CreatePerspectiveFieldOfView((float)fov, Aspect, 0.01f, ZFar);
                perspective = Matrix4.CreateScale(transform.Scale.X, transform.Scale.Y, Math.Abs(transform.Size)) * perspective;
                eye = new Vector3(transform.Position) + new Vector3(0, 0, (float)GetWorldZ());
            }
            return Matrix4.LookAt(eye, new Vector3(transform.Position) + lookat, new Vector3(GetUp())) * perspective;
        }

        public double GetWorldZ()
        {
            return Math.Abs(GetWorldTransform().Size / (2 * Math.Tan(fov / 2))); 
        }

        private Vector2 GetUp()
        {
            Matrix4 m = Matrix4.CreateRotationZ(GetWorldTransform().Rotation);
            return Vector2Ext.Transform(new Vector2(0, 1), m);
        }

        public Vector2 WorldToScreen(Vector2 worldCoord)
        {
            return Vector2Ext.Transform(worldCoord, GetWorldToScreenMatrix());
        }

        public Vector2[] WorldToScreen(Vector2[] worldCoord)
        {
            return Vector2Ext.Transform(worldCoord, GetWorldToScreenMatrix());
        }

        public Vector2 ScreenToWorld(Vector2 screenCoord)
        {
            return Vector2Ext.Transform(screenCoord, GetWorldToScreenMatrix().Inverted());
        }

        public Vector2[] ScreenToWorld(Vector2[] screenCoord)
        {
            return Vector2Ext.Transform(screenCoord, GetWorldToScreenMatrix().Inverted());
        }

        private Matrix4 GetWorldToScreenMatrix()
        {
            Matrix4 scale = Matrix4.CreateScale((float)(Controller.CanvasSize.Width / 2), -(float)(Controller.CanvasSize.Height / 2), 1);
            Matrix4 translation = Matrix4.CreateTranslation(new Vector3(1f, -1f, 0f));
            return GetViewMatrix() * translation * scale;
        }

        private Matrix4 GetWorldToClipMatrix()
        {
            Matrix4 scale = Matrix4.CreateScale(1, -1, 1);
            Matrix4 translation = Matrix4.CreateTranslation(new Vector3(0f, 0f, 0f));
            return GetViewMatrix() * translation * scale;
        }

        public Vector2[] GetVerts()
        {
            return new Vector2[4] {
                new Vector2(-1f, -1f),
                new Vector2(1f, -1f),
                new Vector2(1f, 1f),
                new Vector2(-1f, 1f),
            };
        }

        public Vector2[] GetWorldVerts()
        {
            return Vector2Ext.Transform(GetVerts(), GetWorldToClipMatrix().Inverted());
        }

        public float UnitZToWorld(float z)
        {
            return (1 - z) * (float)GetWorldZ(); //(float)(z * GetWorldZ());//
        }

        //get xy world offset needed to make v appear to overlap target in screen space.
        public Vector2 GetOverlapOffset(Vector3 v, Vector3 target)
        {
            Vector3 cameraPos = new Vector3(Transform2.GetPosition(this));
            cameraPos.Z = (float)GetWorldZ();
            float x = (v.X - cameraPos.X) / (v.Z - cameraPos.Z) - (target.X - cameraPos.X) / (target.Z - cameraPos.Z);
            float y = (v.Y - cameraPos.Y) / (v.Z - cameraPos.Z) - (target.Y - cameraPos.Y) / (target.Z - cameraPos.Z);
            Vector2 offset = -new Vector2(x, y) * (v.Z - cameraPos.Z);

            return offset;
        }
    }
}

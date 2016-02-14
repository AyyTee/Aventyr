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
        /// <summary>View offset in clip space coordinates [-1,1].</summary>
        [DataMember]
        public Vector2 ViewOffset { get; set; }
        [DataMember]
        public float ZNear { get; set; }
        [DataMember]
        public float ZFar { get; set; }

        public const double fov = Math.PI/4;

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
            ViewOffset = new Vector2();
        }
        #endregion

        public override IDeepClone ShallowClone()
        {
            Camera2 clone = new Camera2(Scene);
            ShallowClone(clone);
            return clone;
        }

        protected override void ShallowClone(SceneNode destination)
        {
            base.ShallowClone(destination);
            Camera2 destinationCast = (Camera2)destination;
            destinationCast.Aspect = Aspect;
            destinationCast.ZNear = ZNear;
            destinationCast.ZFar = ZFar;
            destinationCast.ViewOffset = ViewOffset;
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

            float x, y;
            x = ViewOffset.X / 2;
            y = ViewOffset.Y / 2;
            if (isOrtho)
            {
                float width, height;
                width = transform.Scale.X * Aspect;
                height = transform.Scale.Y;
                x *= transform.Scale.X * Aspect;
                y *= transform.Scale.Y;
                perspective = Matrix4.CreateOrthographicOffCenter(x - width / 2, x + width / 2, y - height / 2, y + height / 2, ZNear, ZFar);
                eye = new Vector3(transform.Position) + new Vector3(0, 0, 5000);
            }
            else
            {
                //For some reason things don't line up unless we scale by this value.
                float a = (float)(Math.Tan(fov / 2) / 50);
                x *= Aspect / a;
                perspective = Matrix4.CreatePerspectiveOffCenter(-Aspect * a / 2 + x, Aspect * a / 2 + x, -a / 2 + y, a / 2 + y, 0.01f, ZFar);
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
            return (1 - z) * (float)GetWorldZ();
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

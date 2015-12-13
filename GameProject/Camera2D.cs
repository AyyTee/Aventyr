using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Camera2D : Placeable2D
    {
        public float Aspect = 1;
        public float Scale = 1;
        /// <summary>
        /// Position used for casting line of sight rays for portals
        /// </summary>
        public Vector2 Viewpoint = new Vector2();
        public float ZNear { get; set; }
        public float ZFar { get; set; }

        public Camera2D(Vector2 position, float scale, float aspectRatio)
            : this(new Transform2D(position), scale, aspectRatio)
        {
        }

        public Camera2D(Transform2D transform, float scale, float aspectRatio)
        {
            SetTransform(transform);
            Aspect = aspectRatio;
            Scale = scale;
            ZNear = -10000f;
            ZFar = 10000f;
        }

        /// <summary>
        /// Create a view matrix for this Camera
        /// </summary>
        /// <returns>A view matrix to look in the camera's direction</returns>
        public Matrix4 GetViewMatrix()
        {
            Transform2D transform = GetWorldTransform();
            Matrix4 m = Matrix4.CreateRotationZ(transform.Rotation);
            Vector3 lookat = Vector3.Transform(new Vector3(0, 0, -1), m);
            Matrix4 perspective = Matrix4.CreateOrthographic(transform.Scale.X * Scale * Aspect, transform.Scale.Y * Scale, ZNear, ZFar);
            Vector3 eye = new Vector3(transform.Position) + new Vector3(0, 0, 5000);
            return Matrix4.LookAt(eye, new Vector3(transform.Position) + lookat, GetUp()) * perspective;
        }

        public Vector3 GetUp()
        {
            Matrix4 m = Matrix4.CreateRotationZ(GetWorldTransform().Rotation);
            return Vector3.Transform(new Vector3(0, 1, 0), m);
        }

        public Vector3 GetRight()
        {
            Matrix4 m = Matrix4.CreateRotationZ(GetWorldTransform().Rotation);
            return Vector3.Transform(new Vector3(1, 0, 0), m);
        }

        public Vector2 WorldToScreen(Vector2 worldCoord)
        {
            return Vector2Ext.Transform(worldCoord, GetWorldToScreenMatrix());
        }

        public Vector2 ScreenToWorld(Vector2 screenCoord)
        {
            return Vector2Ext.Transform(screenCoord, GetWorldToScreenMatrix().Inverted());
        }

        private Matrix4 GetWorldToScreenMatrix()
        {
            Matrix4 scale = Matrix4.CreateScale((float)(Controller.CanvasSize.Width / 2), -(float)(Controller.CanvasSize.Height / 2), 1);
            Matrix4 translation = Matrix4.CreateTranslation(new Vector3(1f, -1f, 0f));
            return GetViewMatrix() * translation * scale;
        }
    }
}

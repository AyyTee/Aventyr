using OpenTK;
using System;
using System.Diagnostics;

namespace Game
{
    public class Camera
    {
        public float Aspect = 1;
        public float Scale = 1;
        /// <summary>
        /// Position used for casting line of sight rays for portals
        /// </summary>
        public Vector2 Viewpoint = new Vector2();
        public Transform3 Transform { get; set; }
        private float _fov = 35;
        public float FOV 
        {
            get { return _fov; }
            set
            {
                _fov = (float)MathHelper.Clamp(value, float.Epsilon, Math.PI - 0.1);
            }
        }
        public float ZNear { get; set; }
        public float ZFar { get; set; }
        public bool Orthographic { get; set; }

        public Camera()
        {
            Transform = new Transform3();
        }

        public static Camera CameraOrtho(Vector3 position, float scale, float aspect)
        {
            Camera cam = new Camera();
            cam.Transform.Rotation = new Quaternion(0, 0, 1, 0);
            cam.Transform.Position = new Vector3(position.X, position.Y, 1000);
            cam.Transform.Scale = new Vector3(1, 1, 1);
            cam.Aspect = aspect;
            cam.Scale = scale;
            cam.Orthographic = true;
            cam.ZNear = -10000f;
            cam.ZFar = 10000f;
            return cam;
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
        
        /// <summary>
        /// Create a view matrix for this Camera
        /// </summary>
        /// <returns>A view matrix to look in the camera's direction</returns>
        public Matrix4 GetViewMatrix()
        {
            Matrix4 m = Matrix4.CreateFromAxisAngle(new Vector3(Transform.Rotation.X, Transform.Rotation.Y, Transform.Rotation.Z), Transform.Rotation.W);
            Vector3 lookat = Vector3.Transform(new Vector3(0, 0, -1), m);
            Matrix4 perspective;
            if (Orthographic)
            {
                perspective = Matrix4.CreateOrthographic(Transform.Scale.X * Scale * Aspect, Transform.Scale.Y * Scale, ZNear, ZFar);
            }
            else
            {
                perspective = Matrix4.CreatePerspectiveFieldOfView(FOV, Transform.Scale.X / Transform.Scale.Y, ZNear, ZFar);
            }
            return Matrix4.LookAt(Transform.Position, Transform.Position + lookat, GetUp()) * perspective;
        }

        public Vector3 GetUp()
        {
            Matrix4 m = Matrix4.CreateFromAxisAngle(new Vector3(Transform.Rotation.X, Transform.Rotation.Y, Transform.Rotation.Z), Transform.Rotation.W);
            return Vector3.Transform(new Vector3(0, 1, 0), m);
        }

        public Vector3 GetRight()
        {
            Matrix4 m = Matrix4.CreateFromAxisAngle(new Vector3(Transform.Rotation.X, Transform.Rotation.Y, Transform.Rotation.Z), Transform.Rotation.W);
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
            Debug.Assert(Orthographic, "Only ortho projection is allowed for now.");
            Matrix4 scale = Matrix4.CreateScale((float)(Controller.CanvasSize.Width / 2), -(float)(Controller.CanvasSize.Height / 2), 1);
            Matrix4 translation = Matrix4.CreateTranslation(new Vector3(1f, -1f, 0f));
            return GetViewMatrix() * translation * scale;
        }

        /*public Camera Clone()
        {
            Camera camera = new Camera();
            //camera.Transform = Transform.Clone();
            return camera;
        }*/
    }
}

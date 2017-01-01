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
        public float Fov 
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
            return new Camera
            {
                Orthographic = true,
                Scale = scale,
                Aspect = aspect,
                Transform =
                {
                    Scale = new Vector3(1, 1, 1),
                    Position = new Vector3(position.X, position.Y, 1000),
                    Rotation = new Quaternion(0, 0, 1, 0)
                },
                ZNear = -10000f,
                ZFar = 10000f
            };
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
            Matrix4 perspective = Orthographic ? 
                Matrix4.CreateOrthographic(Transform.Scale.X * Scale * Aspect, Transform.Scale.Y * Scale, ZNear, ZFar) : 
                Matrix4.CreatePerspectiveFieldOfView(Fov, Transform.Scale.X / Transform.Scale.Y, ZNear, ZFar);
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

        public Vector2 WorldToScreen(Vector2 worldCoord, Vector2 canvasSize)
        {
            return Vector2Ext.Transform(worldCoord, GetWorldToScreenMatrix(canvasSize));
        }

        public Vector2 ScreenToWorld(Vector2 screenCoord, Vector2 canvasSize)
        {
            return Vector2Ext.Transform(screenCoord, GetWorldToScreenMatrix(canvasSize).Inverted());
        }

        private Matrix4 GetWorldToScreenMatrix(Vector2 canvasSize)
        {
            Debug.Assert(Orthographic, "Only ortho projection is allowed for now.");
            var scale = Matrix4.CreateScale((float)(canvasSize.X / 2), -(canvasSize.Y / 2), 1);
            var translation = Matrix4.CreateTranslation(new Vector3(1f, -1f, 0f));
            return GetViewMatrix() * translation * scale;
        }
    }
}

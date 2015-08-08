using OpenTK;
using System;

namespace Game
{
    class Camera
    {
        private Transform _transform = new Transform();
        public Transform Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }
        private float FOV 
        {
            get
            {
                return FOV;
            }
            set
            {
                this.FOV = (float)MathHelper.Clamp(value, float.Epsilon, Math.PI - 0.1);
            }
        }
        public float Aspect { get; set; }
        public float ZNear { get; set; }
        public float ZFar { get; set; }
        public bool Orthographic { get; set; }

        public Camera()
        {
            Transform.FixedScale = true;
        }

        public static Camera CameraOrtho(Vector3 position, float scale, float aspect)
        {
            Camera cam = new Camera();
            cam.Transform.Rotation = new Quaternion(0, 0, 1, 0);
            cam.Transform.Position = position;
            cam.Transform.Scale = new Vector3(scale);
            cam.Aspect = aspect;
            cam.Orthographic = true;
            cam.ZNear = 0.01f;
            cam.ZFar = 10000f;
            return cam;
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
                perspective = Matrix4.CreateOrthographic(Aspect * Transform.Scale.X, Transform.Scale.Y, ZNear, ZFar);
            }
            else
            {
                perspective = Matrix4.CreatePerspectiveFieldOfView(FOV, Aspect, ZNear, ZFar);
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
    }
}

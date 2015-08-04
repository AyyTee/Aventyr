using OpenTK;
using System;

namespace Game
{
    class Camera
    {
        public Vector3 Orientation { get; set; }
        public float Scale { get; set; }
        private float FOV { get; set; }
        public float Aspect { get; set; }
        public float ZNear { get; set; }
        public float ZFar { get; set; }
        public bool Orthographic { get; set; }
        public Vector3 Position { get; set; }
        public static Camera CameraOrtho(Vector3 position, float scale, float aspect)
        {
            Camera cam = new Camera();
            cam.Orientation = new Vector3((float)Math.PI, 0f, 0f);
            cam.Position = position;
            cam.Scale = scale;
            cam.Aspect = aspect;
            cam.Orthographic = true;
            cam.ZNear = 0.01f;
            cam.ZFar = 10000f;
            return cam;
        }
        
        public float GetFOV()
        {
            return FOV;
        }
        public void SetFOV(float FOV)
        {
            this.FOV = (float)MathHelper.Clamp(FOV, float.Epsilon, Math.PI - 0.1);
        }
        /// <summary>
        /// Create a view matrix for this Camera
        /// </summary>
        /// <returns>A view matrix to look in the camera's direction</returns>
        public Matrix4 GetViewMatrix()
        {
            Vector3 lookat = new Vector3();

            lookat.X = (float)(Math.Sin((float)Orientation.X) * Math.Cos((float)Orientation.Y));
            lookat.Y = (float)Math.Sin((float)Orientation.Y);
            lookat.Z = (float)(Math.Cos((float)Orientation.X) * Math.Cos((float)Orientation.Y));
            Matrix4 perspective;
            if (Orthographic)
            {
                perspective = Matrix4.CreateOrthographic(Aspect * Scale, Scale, ZNear, ZFar);
            }
            else
            {
                perspective = Matrix4.CreatePerspectiveFieldOfView(FOV, Aspect, ZNear, ZFar);
            }
            return Matrix4.LookAt(Position, Position + lookat, Vector3.UnitY) * perspective;
        }
    }
}

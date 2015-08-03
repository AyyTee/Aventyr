using OpenTK;
using System;

namespace OpenTKTutorial6
{
    class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Orientation = new Vector3((float)Math.PI, 0f, 0f);
        public float MoveSpeed = 0.2f;
        public float MouseSensitivity = 0.01f;
        public float Scale = 1f;
        private float FOV = 1f;
        public float Aspect = 1f;
        public float ZNear = 0.01f;
        public float ZFar = 10000f;
        public bool Orthographic = false;
        public static Camera CameraOrtho(Vector3 position, float scale, float aspect)
        {
            Camera cam = new Camera();
            cam.Position = position;
            cam.Scale = scale;
            cam.Aspect = aspect;
            cam.Orthographic = true;
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

        /// <summary>
        /// Offset the position of the camera in coordinates relative to its current orientation
        /// </summary>
        /// <param name="x">Movement along the camera ground (left/right)</param>
        /// <param name="y">Movement along the camera axis (forward)</param>
        /// <param name="z">Height to move</param>
        public void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();
            Vector3 forward = new Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        /// <summary>
        /// Adds rotation from mouse movement to camera orientation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}

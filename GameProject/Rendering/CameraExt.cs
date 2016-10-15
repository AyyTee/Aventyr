using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class CameraExt
    {
        /// <summary>
        /// Create a view matrix for this Camera
        /// </summary>
        /// <returns>A view matrix to look in the camera's direction</returns>
        public static Matrix4 GetViewMatrix(ICamera2 camera, bool isOrtho = true)
        {
            Transform2 transform = camera.GetWorldTransform();
            Matrix4 m = Matrix4.CreateRotationZ(transform.Rotation);
            Vector3 lookat = Vector3.Transform(new Vector3(0, 0, -1), m);
            Vector3 eye;
            Matrix4 perspective;

            float x, y;
            x = camera.ViewOffset.X / 2;
            y = camera.ViewOffset.Y / 2;
            if (isOrtho)
            {
                float width, height;
                width = transform.Scale.X * camera.Aspect;
                height = transform.Scale.Y;
                x *= transform.Scale.X * camera.Aspect;
                y *= transform.Scale.Y;
                perspective = Matrix4.CreateOrthographicOffCenter(x - width / 2, x + width / 2, y - height / 2, y + height / 2, camera.ZNear, camera.ZFar);
                eye = new Vector3(transform.Position) + new Vector3(0, 0, 50);
            }
            else
            {
                //For some reason things don't line up unless we scale by this value.
                float a = (float)(Math.Tan(camera.Fov / 2) / 50);
                x *= camera.Aspect / a;
                perspective = Matrix4.CreatePerspectiveOffCenter(-camera.Aspect * a / 2 + x, camera.Aspect * a / 2 + x, -a / 2 + y, a / 2 + y, 0.01f, camera.ZFar);
                perspective = Matrix4.CreateScale(transform.Scale.X, transform.Scale.Y, Math.Abs(transform.Size)) * perspective;
                eye = new Vector3(transform.Position) + new Vector3(0, 0, (float)GetWorldZ(camera));
            }
            return Matrix4.LookAt(eye, new Vector3(transform.Position) + lookat, new Vector3(GetUp(camera))) * perspective;
        }

        public static float UnitZToWorld(ICamera2 camera, float z)
        {
            return (1 - z) * (float)GetWorldZ(camera);
        }

        private static double GetWorldZ(ICamera2 camera)
        {
            return Math.Abs(camera.GetWorldTransform().Size / (2 * Math.Tan(camera.Fov / 2)));
        }

        //get xy world offset needed to make v appear to overlap target in screen space.
        public static Vector2 GetOverlapOffset(ICamera2 camera, Vector3 v, Vector3 target)
        {
            Vector3 cameraPos = new Vector3(camera.GetWorldTransform().Position);
            cameraPos.Z = (float)GetWorldZ(camera);
            float x = (v.X - cameraPos.X) / (v.Z - cameraPos.Z) - (target.X - cameraPos.X) / (target.Z - cameraPos.Z);
            float y = (v.Y - cameraPos.Y) / (v.Z - cameraPos.Z) - (target.Y - cameraPos.Y) / (target.Z - cameraPos.Z);
            Vector2 offset = -new Vector2(x, y) * (v.Z - cameraPos.Z);

            return offset;
        }

        public static Vector2[] GetVerts()
        {
            return new Vector2[4] {
                new Vector2(-1f, -1f),
                new Vector2(1f, -1f),
                new Vector2(1f, 1f),
                new Vector2(-1f, 1f),
            };
        }

        public static Vector2[] GetWorldVerts(ICamera2 camera)
        {
            return Vector2Ext.Transform(GetVerts(), WorldToClipMatrix(camera).Inverted());
        }

        private static Matrix4 WorldToScreenMatrix(ICamera2 camera)
        {
            Matrix4 scale = Matrix4.CreateScale(Controller.CanvasSize.Width / 2, -Controller.CanvasSize.Height / 2, 1);
            Matrix4 translation = Matrix4.CreateTranslation(new Vector3(1f, -1f, 0f));
            return camera.GetViewMatrix() * translation * scale;
        }

        private static Matrix4 WorldToClipMatrix(ICamera2 camera)
        {
            Matrix4 scale = Matrix4.CreateScale(1, -1, 1);
            Matrix4 translation = Matrix4.CreateTranslation(new Vector3(0f, 0f, 0f));
            return camera.GetViewMatrix() * translation * scale;
        }

        public static Vector2 WorldToScreen(ICamera2 camera, Vector2 worldCoord)
        {
            return Vector2Ext.Transform(worldCoord, WorldToScreenMatrix(camera));
        }

        public static Vector2[] WorldToScreen(ICamera2 camera, IList<Vector2> worldCoord)
        {
            return Vector2Ext.Transform(worldCoord, WorldToScreenMatrix(camera)).ToArray();
        }

        public static Vector2 ScreenToWorld(ICamera2 camera, Vector2 screenCoord)
        {
            return Vector2Ext.Transform(screenCoord, WorldToScreenMatrix(camera).Inverted());
        }

        public static Vector2[] ScreenToWorld(ICamera2 camera, IList<Vector2> screenCoord)
        {
            return Vector2Ext.Transform(screenCoord, WorldToScreenMatrix(camera).Inverted()).ToArray();
        }

        public static Vector2 ScreenToClip(ICamera2 camera, Vector2 screenCoord)
        {
            return Vector2Ext.Transform(screenCoord, WorldToScreenMatrix(camera).Inverted() * WorldToClipMatrix(camera));
        }

        public static Vector2[] ScreenToClip(ICamera2 camera, IList<Vector2> screenCoord)
        {
            return Vector2Ext.Transform(screenCoord, WorldToScreenMatrix(camera).Inverted() * WorldToClipMatrix(camera)).ToArray();
        }

        private static Vector2 GetUp(ICamera2 camera)
        {
            Matrix4 m = Matrix4.CreateRotationZ(camera.GetWorldTransform().Rotation);
            return Vector2Ext.Transform(new Vector2(0, 1), m);
        }

        public static Transform2 GetWorldViewpoint(ICamera2 camera)
        {
            return new Transform2(camera.ViewOffset).Transform(camera.GetWorldTransform());
        }
    }
}

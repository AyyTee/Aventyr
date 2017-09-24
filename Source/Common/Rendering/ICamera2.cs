using Game.Common;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Game.Rendering
{
    public interface ICamera2 : IGetWorldTransformVelocity
    {
        float Aspect { get; }
        /// <summary>
        /// View offset in clip space coordinates [-1,1].
        /// </summary>
        Vector2 ViewOffset { get; }
        /// <summary>
        /// Field of view in radians. Only affects perspective view matrices, not orthographic view matrices.
        /// </summary>
        double Fov { get; }
        /// <summary>
        /// Use orthographic projection when rendering. Otherwise use perspective projection.
        /// </summary>
        bool IsOrtho { get; }
    }

    public static class ICamera2Ex
    {
        /// <summary>
        /// Create a view matrix for this Camera
        /// </summary>
        public static Matrix4 GetViewMatrix(this ICamera2 camera, bool isOrtho = true, float zNear = 1, float zFar = 1000)
        {
            DebugEx.Assert(zFar > zNear);
            Transform2 transform = camera.WorldTransform;
            Vector3 lookat = 
                new Vector3(transform.Position) + 
                Vector3Ex.Transform(
                    new Vector3(0, 0, -1), 
                    Matrix4.CreateRotationZ(transform.Rotation));

            var cameraZ = isOrtho ? 50f : (float)camera.GetWorldZ();
            Vector3 eye = new Vector3(transform.Position) + new Vector3(0, 0, cameraZ);

            if (isOrtho)
            {
                float x = camera.ViewOffset.X * 0.5f * transform.Scale.X * camera.Aspect;
                float y = camera.ViewOffset.Y * 0.5f * transform.Scale.Y;
                float width = transform.Scale.X * camera.Aspect;
                float height = transform.Scale.Y;
                return 
                    Matrix4.LookAt(eye, lookat, new Vector3(GetUp(camera))) * 
                    Matrix4.CreateOrthographicOffCenter(x - width / 2, x + width / 2, y - height / 2, y + height / 2, zNear, zFar);
            }

            return
                Matrix4.LookAt(eye, lookat, new Vector3(GetUp(camera))) *
                Matrix4.CreateScale(transform.MirrorX ? -1 : 1, 1, 1) *
                Matrix4.CreatePerspectiveFieldOfView((float)camera.Fov, camera.Aspect, zNear, zFar) * 
                Matrix4.CreateTranslation(new Vector3(-camera.ViewOffset.X, -camera.ViewOffset.Y, 0));
        }

        public static float UnitZToWorld(this ICamera2 camera, float z)
        {
            return (1 - z) * (float)GetWorldZ(camera);
        }

        public static double GetWorldZ(this ICamera2 camera)
        {
            return Math.Abs(camera.WorldTransform.Size / (2 * Math.Tan(camera.Fov / 2)));
        }

        /// <summary>
        /// World offset needed to make v appear to overlap target in screen space.
        /// </summary>
        public static Vector2 GetOverlapOffset(this ICamera2 camera, Vector3 v, Vector3 target)
        {
            Vector3 cameraPos = new Vector3(camera.WorldTransform.Position);
            cameraPos.Z = (float)GetWorldZ(camera);
            float x = (v.X - cameraPos.X) / (v.Z - cameraPos.Z) - (target.X - cameraPos.X) / (target.Z - cameraPos.Z);
            float y = (v.Y - cameraPos.Y) / (v.Z - cameraPos.Z) - (target.Y - cameraPos.Y) / (target.Z - cameraPos.Z);
            Vector2 offset = -new Vector2(x, y) * (v.Z - cameraPos.Z);

            return offset;
        }

        public static Vector2[] GetVerts()
        {
            return new[] {
                new Vector2(-1f, -1f),
                new Vector2(1f, -1f),
                new Vector2(1f, 1f),
                new Vector2(-1f, 1f),
            };
        }

        public static Vector2[] GetWorldVerts(this ICamera2 camera)
        {
            return Vector2Ex.Transform(GetVerts(), WorldToClipMatrix(camera).Inverted());
        }

        static Matrix4 WorldToScreenMatrix(this ICamera2 camera, Vector2i canvasSize)
        {
            Matrix4 scale = Matrix4.CreateScale(canvasSize.X / 2f, -canvasSize.Y / 2f, 1);
            Matrix4 translation = Matrix4.CreateTranslation(new Vector3(1f, -1f, 0f));
            return camera.GetViewMatrix() * translation * scale;
        }

        static Matrix4 WorldToClipMatrix(this ICamera2 camera)
        {
            return camera.GetViewMatrix() * Matrix4.CreateScale(1, -1, 1);
        }

        public static Vector2 WorldToScreen(this ICamera2 camera, Vector2 worldCoord, Vector2i canvasSize)
        {
            return Vector2Ex.Transform(worldCoord, WorldToScreenMatrix(camera, canvasSize));
        }

        public static Vector2[] WorldToScreen(this ICamera2 camera, IList<Vector2> worldCoord, Vector2i canvasSize)
        {
            return Vector2Ex.Transform(worldCoord, WorldToScreenMatrix(camera, canvasSize)).ToArray();
        }

        public static Vector2 ScreenToWorld(this ICamera2 camera, Vector2 screenCoord, Vector2i canvasSize)
        {
            return Vector2Ex.Transform(screenCoord, WorldToScreenMatrix(camera, canvasSize).Inverted());
        }

        public static Vector2[] ScreenToWorld(this ICamera2 camera, IList<Vector2> screenCoord, Vector2i canvasSize)
        {
            return Vector2Ex.Transform(screenCoord, WorldToScreenMatrix(camera, canvasSize).Inverted()).ToArray();
        }

        public static Vector2 ScreenToClip(this ICamera2 camera, Vector2 screenCoord, Vector2i canvasSize)
        {
            return Vector2Ex.Transform(screenCoord, WorldToScreenMatrix(camera, canvasSize).Inverted() * GetViewMatrix(camera));
        }

        public static Vector2[] ScreenToClip(this ICamera2 camera, IList<Vector2> screenCoord, Vector2i canvasSize)
        {
            return Vector2Ex.Transform(screenCoord, WorldToScreenMatrix(camera, canvasSize).Inverted() * GetViewMatrix(camera)).ToArray();
        }

        public static Vector2 ClipToWorld(this ICamera2 camera, Vector2 screenCoord)
        {
            return Vector2Ex.Transform(screenCoord, GetViewMatrix(camera).Inverted());
        }

        public static Vector2[] ClipToWorld(this ICamera2 camera, IList<Vector2> screenCoord)
        {
            return Vector2Ex.Transform(screenCoord, GetViewMatrix(camera).Inverted()).ToArray();
        }

        static Vector2 GetUp(this ICamera2 camera)
        {
            Matrix4 m = Matrix4.CreateRotationZ(camera.WorldTransform.Rotation);
            return Vector2Ex.Transform(new Vector2(0, 1), m);
        }

        public static Transform2 GetWorldViewpoint(this ICamera2 camera)
        {
            return new Transform2(camera.ViewOffset).Transform(camera.WorldTransform);
        }
    }
}

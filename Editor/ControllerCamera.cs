using Game;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class ControllerCamera
    {
        public Camera Camera { get; private set; }
        public InputExt InputExt { get; private set; }
        public float ZoomMin = 0.5f;
        public float ZoomMax = 1000f;
        public float KeyMoveSpeed = 0.1f;

        private float _zoomScrollFactor;
        /// <summary>
        /// How much the camera zooms in/out with mouse scrolling. Value must be greater than 1.
        /// </summary>
        public float ZoomScrollFactor 
        { 
            get { return _zoomScrollFactor; }
            set 
            {  
                Debug.Assert(value > 1);
                _zoomScrollFactor = value;
            }
        }

        private float _zoomFactor;
        /// <summary>
        /// How much the camera zooms in/out with key input. Value must be greater than 1.
        /// </summary>
        public float ZoomFactor
        {
            get { return _zoomFactor; }
            set 
            {  
                Debug.Assert(value > 1);
                _zoomFactor = value;
            }
        }

        Vector2 _mouseDragPos;
        Vector3 _cameraDragPos;
        public ControllerCamera(Camera camera, InputExt inputExt)
        {
            ZoomScrollFactor = 1.2f;
            ZoomFactor = 1.5f;
            Camera = camera;
            InputExt = inputExt;
        }

        public void Update()
        {
            if (InputExt.MouseInside)
            {
                if (InputExt.MouseDown(MouseButton.Middle))
                {
                    if (InputExt.MousePress(MouseButton.Middle))
                    {
                        _mouseDragPos = Camera.ScreenToWorld(InputExt.MousePos);
                        _cameraDragPos = Camera.Transform.Position;
                    }
                    Vector3 camPosPrev = Camera.Transform.Position;
                    Camera.Transform.Position = _cameraDragPos;
                    Vector2 offset = _mouseDragPos - Camera.ScreenToWorld(InputExt.MousePos);
                    Camera.Transform.Position = camPosPrev;
                    Camera.Transform.Position = _cameraDragPos + new Vector3(offset.X, offset.Y, 0);
                }
                else
                {
                    Vector2 mouseZoomPosPrev = Camera.ScreenToWorld(InputExt.MousePos);
                    Camera.Scale = MathHelper.Clamp(Camera.Scale / (float)Math.Pow(ZoomScrollFactor, InputExt.MouseWheelDelta()), ZoomMin, ZoomMax);
                    Vector2 mouseZoomPos = -Camera.ScreenToWorld(InputExt.MousePos) + mouseZoomPosPrev;
                    Camera.Transform.Position += new Vector3(mouseZoomPos.X, mouseZoomPos.Y, 0);
                }
            }
            if (InputExt.KeyPress(Key.KeypadPlus) || InputExt.KeyPress(Key.Plus))
            {
                Camera.Scale = MathHelper.Clamp(Camera.Scale / ZoomFactor, ZoomMin, ZoomMax);
            }
            if (InputExt.KeyPress(Key.KeypadMinus) || InputExt.KeyPress(Key.Minus))
            {
                Camera.Scale = MathHelper.Clamp(Camera.Scale * ZoomFactor, ZoomMin, ZoomMax);
            }
            if (InputExt.KeyDown(Key.A))
            {
                Camera.Transform.Rotation += new Quaternion(0, 0, 0, .01f);
            }
            if (InputExt.KeyPress(Key.Space))
            {
                Camera.Transform.Rotation = new Quaternion(0, 0, 1, 0);
            }
            if (InputExt.KeyDown(Key.Left))
            {
                Camera.Transform.Position += new Vector3(-KeyMoveSpeed, 0, 0);
            }
            if (InputExt.KeyDown(Key.Right))
            {
                Camera.Transform.Position += new Vector3(KeyMoveSpeed, 0, 0);
            }
            if (InputExt.KeyDown(Key.Up))
            {
                Camera.Transform.Position += new Vector3(0, KeyMoveSpeed, 0);
            }
            if (InputExt.KeyDown(Key.Down))
            {
                Camera.Transform.Position += new Vector3(0, -KeyMoveSpeed, 0);
            }
        }
    }
}

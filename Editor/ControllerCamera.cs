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
        public delegate void CameraObjectHandler(ControllerCamera controller, Camera2D camera);
        /// <summary>
        /// Event is fired if the camera Transform is modified by this controller.
        /// </summary>
        public event CameraObjectHandler CameraMoved;

        public ControllerEditor Controller { get; private set; }
        public Camera2D Camera { get; private set; }
        public InputExt InputExt { get; private set; }
        public float ZoomMin = 0.5f;
        public float ZoomMax = 1000f;
        public float KeyMoveSpeed = 0.13f;

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
        Vector2 _cameraDragPos;
        public ControllerCamera(ControllerEditor controller, Camera2D camera, InputExt inputExt)
        {
            Controller = controller;
            ZoomScrollFactor = 1.2f;
            ZoomFactor = 1.5f;
            Camera = camera;
            InputExt = inputExt;
        }

        public void Update()
        {
            Transform2D previous = Camera.GetTransform();
            Transform2D transform = Camera.GetTransform();
            if (InputExt.MouseInside)
            {
                if (InputExt.MouseDown(MouseButton.Middle))
                {
                    if (InputExt.MousePress(MouseButton.Middle))
                    {
                        _mouseDragPos = Camera.ScreenToWorld(InputExt.MousePos);
                        _cameraDragPos = transform.Position;
                    }
                    Vector2 camPosPrev = transform.Position;
                    Camera.SetPosition(_cameraDragPos);
                    Vector2 offset = _mouseDragPos - Camera.ScreenToWorld(InputExt.MousePos);
                    Camera.SetPosition(camPosPrev);
                    transform.Position = _cameraDragPos + offset;
                }
                else
                {
                    Vector2 mouseZoomPosPrev = Camera.ScreenToWorld(InputExt.MousePos);
                    Camera.Scale = MathHelper.Clamp(Camera.Scale / (float)Math.Pow(ZoomScrollFactor, InputExt.MouseWheelDelta()), ZoomMin, ZoomMax);
                    Vector2 mouseZoomPos = -Camera.ScreenToWorld(InputExt.MousePos) + mouseZoomPosPrev;
                    transform.Position += mouseZoomPos;
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
            if (InputExt.KeyPress(Key.Space))
            {
                transform.Rotation = 0;
                EditorObject selected = Controller.GetSelectedEntity();
                if (selected != null)
                {
                    /*Vector3 position = copy.Position;
                    position.X = selected.GetTransform().Position.X;
                    position.Y = selected.GetTransform().Position.Y;
                    Camera.Transform.Position = position;*/
                    transform.Position = selected.GetTransform().Position;
                }
            }
            if (InputExt.KeyDown(Key.Left))
            {
                //Camera.Transform.Position += new Vector3(-KeyMoveSpeed, 0, 0);
                transform.Position += new Vector2(-KeyMoveSpeed, 0);
            }
            if (InputExt.KeyDown(Key.Right))
            {
                //Camera.Transform.Position += new Vector3(KeyMoveSpeed, 0, 0);
                transform.Position += new Vector2(KeyMoveSpeed, 0);
            }
            if (InputExt.KeyDown(Key.Up))
            {
                //Camera.Transform.Position += new Vector3(0, KeyMoveSpeed, 0);
                transform.Position += new Vector2(0, KeyMoveSpeed);
            }
            if (InputExt.KeyDown(Key.Down))
            {
                //Camera.Transform.Position += new Vector3(0, -KeyMoveSpeed, 0);
                transform.Position += new Vector2(0, -KeyMoveSpeed);
            }
            Camera.SetTransform(transform);
            if (!transform.Compare(previous) && CameraMoved != null)
            {
                CameraMoved(this, Camera);
            }
            Camera.Viewpoint = new Vector2(transform.Position.X, transform.Position.Y);
        }
    }
}

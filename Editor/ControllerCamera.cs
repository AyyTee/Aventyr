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
        public delegate void CameraObjectHandler(ControllerCamera controller, Camera2 camera);
        /// <summary>Event is fired if the camera Transform is modified by this controller.</summary>
        public event CameraObjectHandler CameraMoved;

        public ControllerEditor Controller { get; private set; }
        public Camera2 Camera { get; private set; }
        public InputExt InputExt { get; private set; }
        public float ZoomMin = 0.5f;
        public float ZoomMax = 1000f;
        public float KeyMoveSpeed = 0.13f;
        Queue<Vector2> lazyPan = new Queue<Vector2>();

        private float _zoomScrollFactor;
        /// <summary>How much the camera zooms in/out with mouse scrolling. Value must be greater than 1.</summary>
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
        /// <summary>How much the camera zooms in/out with key input. Value must be greater than 1.</summary>
        public float ZoomFactor
        {
            get { return _zoomFactor; }
            set 
            {  
                Debug.Assert(value > 1);
                _zoomFactor = value;
            }
        }

        public ControllerCamera(ControllerEditor controller, Camera2 camera, InputExt inputExt)
        {
            Controller = controller;
            ZoomScrollFactor = 1.2f;
            ZoomFactor = 1.5f;
            Camera = camera;
            camera.PortalEnter += portalEnterCallback;
            InputExt = inputExt;
            for (int i = 0; i < 3; i++)
            {
                lazyPan.Enqueue(new Vector2());
            }
        }

        private void portalEnterCallback(SceneNodePlaceable placeable, Portal portalEnter)
        {
            Vector2[] list = lazyPan.ToArray();
            portalEnter.EnterVelocity(list);
            lazyPan = new Queue<Vector2>(list);
        }

        public void Update()
        {
            bool isMoved = false;

            //Handle user input for zooming the camera.
            {
                if (InputExt.MouseInside)
                {
                    /*Vector2 mouseZoomPosPrev = Camera.ScreenToWorld(InputExt.MousePos);
                    Camera.Scale = MathHelper.Clamp(Camera.Scale / (float)Math.Pow(ZoomScrollFactor, InputExt.MouseWheelDelta()), ZoomMin, ZoomMax);
                    Vector2 mouseZoomPos = -Camera.ScreenToWorld(InputExt.MousePos) + mouseZoomPosPrev;
                    transform.Position += mouseZoomPos;*/
                    if (InputExt.MouseWheelDelta() != 0)
                    {
                        Camera.Zoom = MathHelper.Clamp(Camera.Zoom / (float)Math.Pow(ZoomScrollFactor, InputExt.MouseWheelDelta()), ZoomMin, ZoomMax);
                        //Transform2.SetScale(Camera, MathHelper.Clamp(Transform2.GetScale(Camera) / (float)Math.Pow(ZoomScrollFactor, InputExt.MouseWheelDelta()), ZoomMin, ZoomMax);
                        isMoved = true;
                    }
                }
                if (InputExt.KeyPress(Key.KeypadPlus) || InputExt.KeyPress(Key.Plus))
                {
                    //Camera.Zoom = MathHelper.Clamp(Camera.Zoom / ZoomFactor, ZoomMin, ZoomMax);
                    isMoved = true;
                }
                if (InputExt.KeyPress(Key.KeypadMinus) || InputExt.KeyPress(Key.Minus))
                {
                    //Camera.Zoom = MathHelper.Clamp(Camera.Zoom * ZoomFactor, ZoomMin, ZoomMax);
                    isMoved = true;
                }
            }

            //Handle user input to reset the camera's orientation and center it on the current selected object if it exists.
            if (InputExt.KeyPress(Key.Space))
            {
                Transform2 transform = Camera.GetTransform();
                transform.Rotation = 0;
                transform.Scale = new Vector2(Math.Abs(transform.Scale.X), Math.Abs(transform.Scale.Y));
                EditorObject selected = Controller.selection.First;
                if (selected != null)
                {
                    transform.Position = selected.GetTransform().Position;
                    if (selected.GetType() == typeof(EditorPortal))
                    {
                        transform.Position += selected.GetWorldTransform().GetRight() * Portal.EnterMinDistance;
                    }
                }
                Camera.SetTransform(transform);
                isMoved = true;
            }

            //Handle user input to pan the camera.
            {
                Vector2 v = new Vector2();
                if (InputExt.KeyDown(Key.Left))
                {
                    v += Camera.GetTransform().GetRight() * -KeyMoveSpeed * Transform2.GetScale(Camera).Length;
                }
                if (InputExt.KeyDown(Key.Right))
                {
                    v += Camera.GetTransform().GetRight() * KeyMoveSpeed * Transform2.GetScale(Camera).Length;
                }
                if (InputExt.KeyDown(Key.Up))
                {
                    v += Camera.GetTransform().GetUp() * KeyMoveSpeed * Transform2.GetScale(Camera).Length;
                }
                if (InputExt.KeyDown(Key.Down))
                {
                    v += Camera.GetTransform().GetUp() * -KeyMoveSpeed * Transform2.GetScale(Camera).Length;
                }
                if (InputExt.MouseInside && InputExt.MouseDown(MouseButton.Middle))
                {
                    lazyPan.Enqueue(Camera.ScreenToWorld(InputExt.MousePosPrev - InputExt.MousePos) - Camera.ScreenToWorld(new Vector2()));
                }
                else
                {
                    lazyPan.Enqueue(v);
                }
                lazyPan.Dequeue();
            }
            
            //Update the camera's velocity.
            Vector2 velocity = lazyPan.Aggregate((item, acc) => item + acc) / lazyPan.Count;
            Camera.SetVelocity(new Transform2(velocity));

            //If the camera has been moved then call events.
            if ((isMoved || velocity != new Vector2()) && CameraMoved != null)
            {
                CameraMoved(this, Camera);
            }
        }
    }
}

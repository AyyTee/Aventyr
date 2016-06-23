using Game;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Game.InputExt;

namespace EditorLogic
{
    [DataContract, AffineMember]
    public class ControllerCamera : ICamera2, IPortalable, IShallowClone<ControllerCamera>, IStep
    {
        public delegate void CameraObjectHandler(ControllerCamera camera);
        /// <summary>Event is fired if the camera Transform is modified by this controller.</summary>
        public event CameraObjectHandler CameraMoved;

        public ControllerEditor Controller { get; set; }
        public InputExt InputExt { get; set; }
        [DataMember]
        public float ZoomMin = 0.5f;
        [DataMember]
        public float ZoomMax = 1000f;
        [DataMember]
        public float KeyMoveSpeed = 0.013f;
        [DataMember]
        Queue<Vector2> lazyPan = new Queue<Vector2>();
        [DataMember]
        Transform2 _transform = new Transform2();
        const int QUEUE_SIZE = 3;
        [DataMember]
        public IScene Scene { get; set; }
        [DataMember]
        private float _zoomScrollFactor;
        [DataMember]
        public Action<IPortal, Transform2, Transform2> enterPortal { get; set; }
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
        [DataMember]
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
        public float Aspect { get { return Controller.CanvasAspect; } }
        public Vector2 ViewOffset { get { return new Vector2(); } }
        public double Fov { get { return Math.PI / 4; } }
        public float ZNear { get { return -1000f; } }
        public float ZFar { get { return 1000f; } }

        public ControllerCamera(ControllerEditor controller, InputExt inputExt, IScene scene)
        {
            Scene = scene;
            Controller = controller;
            _transform.Size = 1;
            ZoomScrollFactor = 1.2f;
            ZoomFactor = 1.5f;
            InputExt = inputExt;
            for (int i = 0; i < QUEUE_SIZE; i++)
            {
                lazyPan.Enqueue(new Vector2());
            }

            /*Entity viewCenter = new Entity(Camera.Scene);
            viewCenter.AddModel(ModelFactory.CreateCircle(new Vector3(), 0.005f, 10));
            viewCenter.DrawOverPortals = true;
            viewCenter.ModelList[0].SetColor(new Vector3(1, 0.9f, 0.2f));
            viewCenter.ModelList[0].Transform.Position = new Vector3(0, 0, DrawDepth.CameraMarker);
            viewCenter.SetParent(Camera);*/
        }

        /*private void portalEnterCallback(SceneNodePlaceable placeable, IPortal portalEnter)
        {
            Vector2[] list = lazyPan.ToArray();
            Portal.EnterVelocity(portalEnter, list);
            lazyPan = new Queue<Vector2>(list);
        }*/

        public Matrix4 GetViewMatrix(bool isOrtho = true)
        {
            return CameraExt.GetViewMatrix(this, isOrtho);
        }

        public Transform2 GetWorldVelocity()
        {
            return GetVelocity();
        }

        public Transform2 GetWorldTransform()
        {
            return GetTransform();
        }

        public Transform2 GetTransform()
        {
            return _transform.ShallowClone();
        }

        public void SetTransform(Transform2 transform)
        {
            _transform = transform.ShallowClone();
        }

        public bool IsLocked()
        {
            return Controller.ActiveTool.LockCamera();
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            if (IsLocked())
            {
                return;
            }
            bool isMoved = false;

            //Handle user input for zooming the camera.
            {
                float scale = Transform2.GetSize(this);
                if (InputExt.MouseInside)
                {
                    if (InputExt.MouseWheelDelta() != 0)
                    {
                        scale /= (float)Math.Pow(ZoomScrollFactor, InputExt.MouseWheelDelta());
                        isMoved = true;
                    }
                }
                if (InputExt.KeyPress(Key.KeypadPlus) || InputExt.KeyPress(Key.Plus))
                {
                    scale /= ZoomFactor;
                    isMoved = true;
                }
                if (InputExt.KeyPress(Key.KeypadMinus) || InputExt.KeyPress(Key.Minus))
                {
                    scale *= ZoomFactor;
                    isMoved = true;
                }
                scale = MathHelper.Clamp(Math.Abs(scale), ZoomMin, ZoomMax) * Math.Sign(Transform2.GetSize(this));
                Transform2.SetSize(this, scale);
            }

            //Handle user input to reset the camera's orientation and center it on the current selected object if it exists.
            if (InputExt.KeyPress(Key.Space))
            {
                Transform2 transform = GetTransform();
                transform.Rotation = 0;
                //transform.Scale = new Vector2(Math.Abs(transform.Scale.X), Math.Abs(transform.Scale.Y));
                transform.MirrorX = false;
                transform.Size = Math.Abs(transform.Size);
                EditorObject selected = Controller.selection.First;
                if (selected != null)
                {
                    transform.Position = selected.GetTransform().Position;
                    if (selected is EditorPortal)
                    {
                        transform.Position += selected.GetWorldTransform().GetRight() * Portal.EnterMinDistance;
                    }
                }
                SetTransform(transform);
                isMoved = true;
            }

            //Handle user input to pan the camera.
            {
                Vector2 v = new Vector2();
                if (!InputExt.KeyDown(KeyBoth.Alt))
                {
                    if (InputExt.KeyDown(Key.Left))
                    {
                        v += GetTransform().GetRight() * -KeyMoveSpeed * Math.Abs(Transform2.GetSize(this));
                    }
                    if (InputExt.KeyDown(Key.Right))
                    {
                        v += GetTransform().GetRight() * KeyMoveSpeed * Math.Abs(Transform2.GetSize(this));
                    }
                    if (InputExt.KeyDown(Key.Up))
                    {
                        v += GetTransform().GetUp() * KeyMoveSpeed * Math.Abs(Transform2.GetSize(this));
                    }
                    if (InputExt.KeyDown(Key.Down))
                    {
                        v += GetTransform().GetUp() * -KeyMoveSpeed * Math.Abs(Transform2.GetSize(this));
                    }
                }
                if (InputExt.MouseInside && InputExt.MouseDown(MouseButton.Middle))
                {
                    lazyPan.Enqueue(CameraExt.ScreenToWorld(this, InputExt.MousePosPrev - InputExt.MousePos) - CameraExt.ScreenToWorld(this, new Vector2()));
                }
                else
                {
                    lazyPan.Enqueue(v);
                }
                lazyPan.Dequeue();
            }

            //If the camera has been moved then call events.
            if ((isMoved || GetWorldVelocity().Position != new Vector2()) && CameraMoved != null)
            {
                CameraMoved(this);
            }
        }

        public void StepEnd(IScene scene, float stepSize)
        {
            if (Controller.renderer.PortalRenderEnabled)
            {
                SceneExt.RayCast(this, Scene.GetPortalList(), 1, true);
            }
            else
            {
                _transform = _transform.Add(GetWorldVelocity());
            }
        }

        public Transform2 GetVelocity()
        {
            Vector2 velocity = lazyPan.Aggregate((item, acc) => item + acc) / lazyPan.Count;
            return Transform2.CreateVelocity(velocity);
        }

        public void SetVelocity(Transform2 velocity)
        {
            lazyPan = new Queue<Vector2>();
            for (int i = 0; i < QUEUE_SIZE; i++)
            {
                lazyPan.Enqueue(velocity.Position);
            }
        }

        public ControllerCamera ShallowClone()
        {
            ControllerCamera clone = new ControllerCamera(Controller, InputExt, Scene);
            clone.lazyPan = new Queue<Vector2>(lazyPan);
            clone._zoomFactor = _zoomFactor;
            clone.ZoomMin = ZoomMin;
            clone.ZoomMax = ZoomMax;
            clone.KeyMoveSpeed = KeyMoveSpeed;
            clone._transform = _transform;
            return clone;
        }
    }
}

using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Physics;
using Game.Portals;
using Game.Rendering;

namespace Game
{
    public class Player : IStep, ISceneObject
    {
        public Actor Actor { get; private set; }
        public Controller Controller;
        public Camera2 Camera;
        public bool FollowPlayer = true;

        public Player(Controller controller)
        {
            Controller = controller;
        }

        public void Remove()
        {
        }

        public void SetActor(Actor actor)
        {
            if (Actor != null)
            {
                Actor.EnterPortal -= EnterPortal;
            }
            Actor = actor;
            Actor.EnterPortal += EnterPortal;
        }

        private void EnterPortal(EnterCallbackData data, Transform2 transformPrevious, Transform2 velocityPrevious)
        {
            if (Camera != null)
            {
                Camera.WorldTransform = Portal.Enter(data.EntrancePortal, Camera.WorldTransform);
            }
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            if (Controller.Input != null)
            {
                if (FollowPlayer)
                {
                    if (KeyLeftDown() != KeyRightDown())
                    {
                        if (KeyLeftDown())
                        {
                            Actor.ApplyForce(new Vector2(-10, 0));
                        }
                        else
                        {
                            Actor.ApplyForce(new Vector2(10, 0));
                        }
                    }

                    if (Camera != null)
                    {
                        Camera.ViewOffset = CameraExt.ScreenToClip(Camera, Controller.Input.MousePos, Vector2Ext.ToOtk(Controller.CanvasSize)) * 0.8f;
                    }
                }
                else
                {

                }
            }
        }

        public bool KeyLeftDown()
        {
            return Controller.Input.KeyDown(Key.Left) || Controller.Input.KeyDown(Key.A); 
        }

        public bool KeyRightDown()
        {
            return Controller.Input.KeyDown(Key.Right) || Controller.Input.KeyDown(Key.D);
        }

        public void StepEnd(IScene scene, float stepSize)
        {
            if (Camera != null)
            {
                Transform2 transform = Camera.WorldTransform;
                transform.Position = Actor.GetTransform().Position;
                Camera.WorldTransform = transform;
            }
        }
    }
}

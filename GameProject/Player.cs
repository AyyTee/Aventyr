using OpenTK;
using OpenTK.Input;
using Game.Common;
using Game.Physics;
using Game.Portals;
using Game.Rendering;

namespace Game
{
    public class Player : IStep, ISceneObject
    {
        public Actor Actor { get; private set; }
        public IVirtualWindow Window;
        public Camera2 Camera;
        public bool FollowPlayer = true;
        public string Name { get; set; } = nameof(Player);

        public Player(IVirtualWindow window)
        {
            Window = window;
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

        void EnterPortal(EnterCallbackData data, Transform2 transformPrevious, Transform2 velocityPrevious)
        {
            if (Camera != null)
            {
                Camera.WorldTransform = Portal.Enter(data.EntrancePortal, Camera.WorldTransform);
            }
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            if (Window != null)
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
                        Camera.ViewOffset = Camera.ScreenToClip(Window.MousePosition, Window.CanvasSize) * 0.8f;
                    }
                }
                else
                {

                }
            }
        }

        public bool KeyLeftDown()
        {
            return Window.ButtonDown(Key.Left) || Window.ButtonDown(Key.A); 
        }

        public bool KeyRightDown()
        {
            return Window.ButtonDown(Key.Right) || Window.ButtonDown(Key.D);
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

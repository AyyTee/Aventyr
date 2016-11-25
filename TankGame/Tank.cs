using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
using Lidgren.Network;

namespace TankGame
{
    public class Tank : IStep, ISceneObject
    {
        public Actor Actor { get; private set; }
        public TankInput Input { get; private set; } = new TankInput();

        public Tank(Scene scene)
        {
            scene.SceneObjectList.Add(this);

            Actor = new Actor(scene, PolygonFactory.CreateRectangle(0.8f, 1));
            Entity entity = new Entity(scene);
            entity.SetParent(Actor);
            entity.AddModel(ModelFactory.CreateCube(new Vector3(0.8f, 1, 1)));
        }

        public void SetInput(TankInput input)
        {
            Input = input;
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            Transform2 transform = Actor.GetTransform();
            Vector2 up = transform.GetUp(true);
            Vector2 right = transform.GetRight(true);
            Transform2 velocity = Actor.GetVelocity();

            if (Input.MoveFoward || Input.MoveBackward)
            {
                float accel = 0.35f;
                if (Input.MoveFoward)
                {
                    velocity.Position += up * accel;
                    //velocity.Position = up;
                }
                else
                {
                    velocity.Position -= up * accel;
                    //velocity.Position = -up;
                }
            }

            if (Input.TurnLeft || Input.TurnRight)
            {
                float rotAccel = 0.55f;
                if (!transform.MirrorX)
                {
                    rotAccel *= -1;
                }
                if (Input.TurnRight)
                {
                    velocity.Rotation += rotAccel;
                }
                else
                {
                    velocity.Rotation -= rotAccel;
                }
            }

            //Apply direction dependent friction.
            Matrix2 friction =
                Matrix2.CreateRotation(-transform.Rotation) *
                Matrix2.CreateScale(0.8f, 0.93f) *
                Matrix2.CreateRotation(transform.Rotation);
            velocity.Position = Vector2Ext.Transform(velocity.Position, friction);

            velocity.Rotation *= 0.8f;

            Actor.SetVelocity(velocity);
        }

        public void StepEnd(IScene scene, float stepSize)
        {
        }
    }
}

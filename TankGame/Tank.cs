using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
using Lidgren.Network;
using TankGame.Network;

namespace TankGame
{
    public class Tank : IStep, ISceneObject, INetObject
    {
        public Actor Actor { get; private set; }
        public TankInput Input { get; private set; } = new TankInput();
        public Entity Turret { get; private set; }
        public double GunFiredTime { get; set; } = -1;
        public int? ServerId { get; set; }

        public Tank(Scene scene)
        {
            scene.SceneObjectList.Add(this);

            Actor = new Actor(scene, PolygonFactory.CreateRectangle(0.8f, 1));
            Entity entity = new Entity(scene);
            entity.SetParent(Actor);
            entity.AddModel(ModelFactory.CreateCube(new Vector3(0.8f, 1, 1)));

            Turret = new Entity(scene);
            Model turretModel = ModelFactory.CreateCube(new Vector3(0.6f, 0.3f, 0.3f));
            turretModel.Transform = new Transform3(new Vector3(0.25f, 0f, 0.5f));
            turretModel.SetColor(new Vector3(0.5f, 0.5f, 0.5f));
            Turret.AddModel(turretModel);
            Turret.SetParent(Actor);
        }

        public void SetInput(TankInput input)
        {
            Input = input;
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            StepTurret(stepSize);
            StepMovement(stepSize);

            double gunReloadTime = 2;
            if ((GunFiredTime + gunReloadTime <= scene.Time || GunFiredTime == -1) && Input.FireGun)
            {
                Transform2 t = Turret.GetWorldTransform();
                new Bullet(Actor.Scene, t.Position, Vector2Ext.LengthDir(2, t.Rotation));
                GunFiredTime = scene.Time;
            }
        }

        private void StepMovement(float stepSize)
        {
            Transform2 transform = Actor.GetTransform();
            Vector2 up = transform.GetUp(true);
            Vector2 right = transform.GetRight(true);
            Transform2 velocity = Actor.GetVelocity();

            if (Input.MoveFoward || Input.MoveBackward)
            {
                float accel = 15f * stepSize;
                if (Input.MoveFoward)
                {
                    velocity.Position += up * accel;
                }
                else
                {
                    velocity.Position -= up * accel;
                }
            }

            if (Input.TurnLeft || Input.TurnRight)
            {
                float rotAccel = 24f * stepSize;
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
                Matrix2.CreateScale((float)Math.Pow(0.000001f, stepSize), (float)Math.Pow(0.001f, stepSize)) *
                Matrix2.CreateRotation(transform.Rotation);
            velocity.Position = Vector2Ext.Transform(velocity.Position, friction);

            velocity.Rotation *= (float)Math.Pow(0.000001f, stepSize);

            Actor.SetVelocity(velocity);
        }

        private void StepTurret(float stepSize)
        {
            float turretSpeed = 1.5f;

            Transform2 t = Turret.GetWorldTransform();
            double angle = MathExt.AngleDiff(t.Rotation, -MathExt.AngleVector(Input.ReticlePos - t.Position));

            Transform2 tLocal = Turret.GetVelocity();
            tLocal.Rotation = Math.Sign(angle) * (float)Math.Min(turretSpeed, Math.Abs(angle / stepSize));
            Turret.SetVelocity(tLocal);
        }

        public void StepEnd(IScene scene, float stepSize)
        {
        }
    }
}

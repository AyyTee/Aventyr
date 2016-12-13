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
using FarseerPhysics.Dynamics;

namespace TankGame
{
    public class Tank : Actor, IStep, ISceneObject, INetObject
    {
        public TankInput Input { get; private set; } = new TankInput();
        public Entity Turret { get; private set; }
        public double GunFiredTime { get; set; } = -1;
        public int? ServerId { get; set; }
        bool _attemptFireGun, _attemptFirePortal0, _attemptFirePortal1;

        public Tank(Scene scene)
            : base(scene, PolygonFactory.CreateRectangle(0.8f, 1))
        {
            scene.SceneObjectList.Add(this);

            //Actor = new Actor(scene, PolygonFactory.CreateRectangle(0.8f, 1));
            Entity entity = new Entity(scene);
            entity.SetParent(this);
            entity.AddModel(ModelFactory.CreateCube(new Vector3(0.8f, 1, 1)));

            Turret = new Entity(scene);
            Model turretModel = ModelFactory.CreateCube(new Vector3(0.6f, 0.3f, 0.3f));
            turretModel.Transform = new Transform3(new Vector3(0.25f, 0f, 0.5f));
            turretModel.SetColor(new Vector3(0.5f, 0.5f, 0.5f));
            Turret.AddModel(turretModel);
            Turret.SetParent(this);

            OnCollision += Collision;

            SetCollisionCategory(Category.Cat1);
            //SetCollidesWith(~Category.Cat1);
        }

        public void SetInput(TankInput input)
        {
            Input = input;

            /*We store attempts to fire weapons here so that if input is set again 
             * we won't ignore the first firing attempt.*/
            _attemptFireGun |= input.FireGun;
            _attemptFirePortal0 |= input.FirePortal0;
            _attemptFirePortal1 |= input.FirePortal1;
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            StepTurret(stepSize);
            StepMovement(stepSize);

            double gunReloadTime = 2;
            if ((GunFiredTime + gunReloadTime <= scene.Time || GunFiredTime == -1) && _attemptFireGun)
            {
                Transform2 t = Turret.GetWorldTransform();
                new Bullet(Scene, t.Position, Vector2Ext.LengthDir(2, t.Rotation));
                GunFiredTime = scene.Time;
            }
            _attemptFireGun = false;
            _attemptFirePortal0 = false;
            _attemptFirePortal1 = false;
        }

        private void StepMovement(float stepSize)
        {
            Transform2 transform = GetTransform();
            Vector2 up = transform.GetUp(true);
            Vector2 right = transform.GetRight(true);
            Transform2 velocity = GetVelocity();

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

            SetVelocity(velocity);
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

        public void Collision(Actor collidingWith, bool isFirst)
        {
        }
    }
}

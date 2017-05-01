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
using Game.Common;
using Game.Models;
using Game.Physics;
using Game.Portals;
using Game.Rendering;
using System.Diagnostics;

namespace TankGame
{
    [DebuggerDisplay(nameof(Tank) + " {" + nameof(Name) + "}")]
    public class Tank : Actor, IStep, INetObject
    {
        public TankInput Input { get; private set; } = new TankInput();
        public Entity Turret { get; private set; }
        public double GunFiredTime { get; set; } = -1;
        public double PortalFiredTime { get; set; } = -1;
        public int? ServerId { get; set; }
        bool _attemptFireGun;
        readonly bool[] _attemptFirePortal = new bool[2];
        public int GunReloadTime = 1;
        public int PortalReloadTime = 1;
        public readonly TankPortal[] PortalPair = new TankPortal[2];

        public Tank(Scene scene)
            : base(scene, PolygonFactory.CreateRectangle(0.8f, 1))
        {
            for (int i = 0; i < PortalPair.Length; i++)
            {
                PortalPair[i] = new TankPortal(Scene);
                var portalEntity = new Entity(Scene);
                portalEntity.SetParent(PortalPair[i]);
                portalEntity.AddModel(ModelFactory.CreateLinesWidth( new[] { new LineF(Portal.GetVerts(PortalPair[i])) }, 0.1f));
                portalEntity.ModelList[0].SetColor(new Vector3(1, 0, 0));
            }
            Portal.SetLinked(PortalPair[0], PortalPair[1]);

            Name = "Player Tank";

            //Actor = new Actor(scene, PolygonFactory.CreateRectangle(0.8f, 1));
            Entity entity = new Entity(scene);
            entity.Name = "Player Tank Entity";
            entity.SetParent(this);
            entity.AddModel(ModelFactory.CreateCube(new Vector3(0.8f, 1, 1)));

            Turret = new Entity(scene);
            Turret.Name = "Tank Turret";
            Model turretModel = ModelFactory.CreateCube(new Vector3(0.6f, 0.3f, 0.3f));
            turretModel.Transform = new Transform3(new Vector3(0.25f, 0f, 0.5f));
            turretModel.SetColor(new Vector3(0.5f, 0.5f, 0.5f));
            Turret.AddModel(turretModel);
            //Turret.SetParent(this);

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
            _attemptFirePortal[0] |= input.FirePortal[0];
            _attemptFirePortal[1] |= input.FirePortal[1];
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            StepTurret(stepSize);
            StepMovement(stepSize);

            if (_attemptFireGun && (GunFiredTime == -1 || GunFiredTime + GunReloadTime <= scene.Time))
            {
                Transform2 t = Turret.GetWorldTransform();
                new Bullet(Scene, t.Position, Vector2Ext.LengthDir(2, t.Rotation));
                GunFiredTime = scene.Time;
            }

            if (PortalFiredTime == -1 || PortalFiredTime + PortalReloadTime <= scene.Time)
            {
                for (int i = 0; i < _attemptFirePortal.Length; i++)
                {
                    if (_attemptFirePortal[i])
                    {
                        Vector2 pos = Turret.WorldTransform.Position;
                        var ray = new LineF(pos, (Input.ReticlePos - pos).Normalized() * 10 + pos);
                        foreach (WallCoord coord in PortalPlacer.PortalPlace(PortalPair[i], ray))
                        {
                            if (coord.Wall is Tank || coord.Wall is Bullet)
                            {
                                continue;
                            }
                            if (PortalPlacer.EdgeValid(coord, PortalPair[i].Size))
                            {
                                PortalCommon.SetLocalTransform(PortalPair[i], PortalPlacer.AdjustCoord(coord, PortalPair[i].Size));
                                break;
                            }
                        }

                        PortalFiredTime = scene.Time;
                        break;
                    }
                }
            }

            _attemptFireGun = false;
            _attemptFirePortal[0] = false;
            _attemptFirePortal[1] = false;
        }

        void StepMovement(float stepSize)
        {
            Transform2 transform = GetTransform();
            Vector2 up = transform.GetUp();
            Vector2 right = transform.GetRight();
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

        void StepTurret(float stepSize)
        {
            float turretSpeed = 1.5f;

            Transform2 t = Turret.GetWorldTransform();
            double angle = MathExt.AngleDiff(t.Rotation, -MathExt.AngleVector(Input.ReticlePos - t.Position));

            Turret.WorldTransform = GetWorldTransform();
            Turret.SetTransform(GetWorldTransform());

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

using FarseerPhysics.Dynamics;
using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankGame.Network;

namespace TankGame
{
    public class Bullet : Actor, IStep, ISceneObject, INetObject
    {
        public Entity Entity { get; private set; }
        public int? ServerId { get; set; }

        public Bullet(Scene scene, Vector2 position, Vector2 velocity)
            : base(scene, PolygonFactory.CreateNGon(3, 0.1f, new Vector2()), new Transform2(position))
        {
            Entity = new Entity(Scene);
            SetVelocity(Transform2.CreateVelocity(velocity));
            Entity.SetParent(this);

            Model model = ModelFactory.CreatePolygon(Vertices, new Vector3(0, 0, 1));
            //Model model = ModelFactory.CreateArrow(new Vector3(0f, 0f, 1f), velocity.Normalized() * 0.1f, 0.1f, 0.3f, 0.15f);
            model.SetColor(new Vector3(1, 1, 0));
            Entity.AddModel(model);

            OnCollision += Bullet_OnCollision;

            SetCollisionCategory(Category.Cat1);
            SetCollidesWith(~Category.Cat1);
        }

        private void Bullet_OnCollision(Actor collidingWith, bool firstEvent)
        {
            if (collidingWith is Wall)
            {
                Scene.MarkForRemoval(this);
            }
        }

        public void StepBegin(IScene scene, float stepSize)
        {
        }

        public void StepEnd(IScene scene, float stepSize)
        {
        }
    }
}

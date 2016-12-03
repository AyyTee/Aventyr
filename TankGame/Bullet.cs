using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    public class Bullet : IStep, ISceneObject
    {
        public Scene Scene { get; private set; }
        public Entity Entity { get; private set; }

        public Bullet(Scene scene, Vector2 position, Vector2 velocity)
        {
            Scene = scene;
            Entity = new Entity(Scene, position);
            Entity.SetVelocity(Transform2.CreateVelocity(velocity));

            Model model = ModelFactory.CreateArrow(new Vector3(0f, 0f, 1f), velocity.Normalized() * 0.1f, 0.1f, 0.3f, 0.15f);
            model.SetColor(new Vector3(1, 1, 0));
            Entity.AddModel(model);
        }

        public void StepBegin(IScene scene, float stepSize)
        {
        }

        public void StepEnd(IScene scene, float stepSize)
        {
            
        }
    }
}

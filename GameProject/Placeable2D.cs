
using FarseerPhysics.Dynamics;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Game
{
    [Serializable]
    public abstract class Placeable2D
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        
        private Transform2D _transform = new Transform2D();
        public virtual Transform2D Transform
        {
            get { return _transform; }
        }

        private Scene _scene = null;
        public Scene Scene
        {
            get { return _scene; }
        }

        public Placeable2D()
        {
        }

        public Placeable2D(Scene scene)
        {
            _scene = scene;
            if (scene != null)
            {
                Id = scene.IdCount;
            }
        }
    }
}

using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class EditorPortal : EditorObject
    {
        public Portal Portal { get; private set; }
        Entity _marker;

        public EditorPortal(Scene scene)
        {
            Portal = new FloatPortal(scene);
            _marker = new Entity(scene);
            //_marker.Models.Add(ModelFactory.CreatePlane(new Vector2(0.1f, 1)));
            _marker.Models.Add(ModelFactory.CreateArrow(new Vector3(0, -0.5f, 0), new Vector2(0, 1), 0.05f, 0.2f, 0.1f));
            _marker.Models.Add(ModelFactory.CreateArrow(new Vector3(), new Vector2(0.2f, 0), 0.05f, 0.2f, 0.1f));
            foreach (Model m in _marker.Models)
            {
                m.SetShader("default");
                m.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
            }
        }

        public void Remove()
        {
            Portal.Scene.RemovePortal(Portal);
            _marker.Scene.RemoveEntity(_marker);
        }

        public override void SetTransform(Transform2D transform)
        {
            if (Portal.GetType() == typeof(FloatPortal))
            {
                FloatPortal portal = (FloatPortal)Portal;
                portal.Transform.SetLocal(transform);
                _marker.Transform.SetLocal(transform);
            }
        }

        public override Transform2D GetTransform()
        {
            return Portal.GetTransform();
        }
    }
}

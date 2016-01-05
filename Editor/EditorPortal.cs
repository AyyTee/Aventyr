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
        public Entity Marker { get; private set; }

        public EditorPortal(ControllerEditor controller, Scene scene, Portal portal)
            : base(controller)
        {
            Portal = portal;
            Marker = new Entity(scene);
            Marker.SetParent(Portal);
            Marker.AddModel(ModelFactory.CreateArrow(new Vector3(0, -0.5f, 0), new Vector2(0, 1), 0.05f, 0.2f, 0.1f));
            Marker.AddModel(ModelFactory.CreateArrow(new Vector3(), new Vector2(0.2f, 0), 0.05f, 0.2f, 0.1f));
            foreach (Model m in Marker.ModelList)
            {
                //m.SetShader("default");
                m.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
            }
        }

        public EditorPortal(ControllerEditor controller, Scene scene)
            : this(controller, scene, new FloatPortal(scene))
        {
        }

        public void Remove()
        {
            Portal.Remove();
            Marker.Remove();
        }

        public override void SetTransform(Transform2D transform)
        {
        }
        public override void SetPosition(Vector2 position)
        {
        }

        public override Transform2D GetTransform()
        {
            return Portal.GetTransform();
        }

        public override Transform2D GetWorldTransform()
        {
            return Portal.GetWorldTransform();
        }
    }
}

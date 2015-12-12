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

        public EditorPortal(Scene scene, Portal portal)
        {
            Portal = portal;
            Marker = new Entity(scene);
            Marker.Transform.Parent = Portal.GetTransform();
            //Marker.Transform.Position = new Vector2(0.001f, 0);
            //_marker.Models.Add(ModelFactory.CreatePlane(new Vector2(0.1f, 1)));
            Marker.Models.Add(ModelFactory.CreateArrow(new Vector3(0, -0.5f, 0), new Vector2(0, 1), 0.05f, 0.2f, 0.1f));
            Marker.Models.Add(ModelFactory.CreateArrow(new Vector3(), new Vector2(0.2f, 0), 0.05f, 0.2f, 0.1f));
            foreach (Model m in Marker.Models)
            {
                m.SetShader("default");
                m.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
            }
        }

        public EditorPortal(Scene scene)
            : this(scene, new FloatPortal(scene))
        {
        }

        public void Remove()
        {
            Portal.Scene.RemovePortal(Portal);
            Marker.Scene.RemoveEntity(Marker);
        }

        public override void SetTransform(Transform2D transform)
        {
            if (Portal.GetType() == typeof(FloatPortal))
            {
                FloatPortal portal = (FloatPortal)Portal;
                portal.Transform.SetLocal(transform);
                //Marker.Transform.SetLocal(transform);
            }
        }

        public override Transform2D GetTransform()
        {
            return Portal.GetTransform();
        }
    }
}

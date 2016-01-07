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
        public FloatPortal Portal { get; private set; }
        public Entity PortalEntity { get; private set; }

        public EditorPortal(ControllerEditor controller)
            : base(controller)
        {
            PortalEntity = new Entity(Scene);
            PortalEntity.SetParent(this);
            Model arrow0, arrow1;
            arrow0 = ModelFactory.CreateArrow(new Vector3(0, -0.5f, 0), new Vector2(0, 1), 0.05f, 0.2f, 0.1f);
            arrow0.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
            PortalEntity.AddModel(arrow0);
            arrow1 = ModelFactory.CreateArrow(new Vector3(), new Vector2(0.2f, 0), 0.05f, 0.2f, 0.1f);
            arrow1.SetColor(new Vector3(0.1f, 0.1f, 0.5f));
            PortalEntity.AddModel(arrow1);

            Portal = new FloatPortal(Scene);
            Portal.SetParent(this);
        }
    }
}

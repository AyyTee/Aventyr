using Game;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    public class Controller : Game.Controller
    {
        public Controller(Window window, string[] args)
            : base(window, args)
        {
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Scene scene = new Scene();
            scene.SetActiveCamera(new Camera2(scene, new Vector2(), CanvasSize.Width / (float)CanvasSize.Height));
            Entity entity = new Entity(scene);
            entity.AddModel(ModelFactory.CreateCube(new Vector3(0.1f, 0.1f, 0.1f)));

            Entity entity2 = new Entity(scene);
            entity2.AddModel(ModelFactory.CreatePlane());
            entity2.ModelList[0].SetTexture(renderer.Textures["default.png"]);

            PortalCommon.UpdateWorldTransform(scene);
            renderer.AddLayer(scene);
        }
    }
}

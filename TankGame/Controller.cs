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
        public Controller(Game.Window window, string[] args)
            : base(window, args)
        {
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Scene scene = new Scene();
            scene.SetActiveCamera(new Camera2(scene, new Vector2(), CanvasSize.Height/(float)CanvasSize.Width));
            Entity entity = new Entity(scene);
            entity.ModelList.Add(ModelFactory.CreateCube());
            PortalCommon.UpdateWorldTransform(scene);
            renderer.AddLayer(scene);
        }
    }
}

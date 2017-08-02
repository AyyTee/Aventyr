using Game;
using Game.Common;
using Game.Portals;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aventyr
{
    public class Controller : IUpdateable
    {
        IVirtualWindow _window;
        Scene _scene;
        Camera2 _camera;

        public Controller(IVirtualWindow window)
        {
            _window = window;

            _scene = new Scene();
            var portal0 = new FloatPortal(_scene, new Transform2(new Vector2(1, 0)));
            var portal1 = new FloatPortal(_scene, new Transform2(new Vector2(2, 0)));
            var portal2 = new FloatPortal(_scene, new Transform2(new Vector2(3, 0)));
            var portal3 = new FloatPortal(_scene, new Transform2(new Vector2(4, 0)));
            var entity = new Entity(_scene, new Transform2(new Vector2(2.5f, 0)));
            var cube = ModelFactory.CreateCube(new Vector3(0.8f), Color4.Green);
            cube.Texture = _window.Textures.Default;
            entity.AddModel(cube);
            Portal.SetLinked(portal0, portal1);
            Portal.SetLinked(portal2, portal3);
            _camera = new Camera2(_scene, new Transform2(new Vector2(), 0, 10), (float)_window.CanvasSize.XRatio);

            PortalCommon.UpdateWorldTransform(_scene);
        }

        public void Render(double timeDelta)
        {
            _window.Layers.Clear();

            var plane = ModelFactory.CreatePlane(new Vector2(10), Color4.LightGray, new Vector3(-5, -5, 0));
            plane.SetTexture(_window.Textures.Grid);
            plane.TransformUv = new Transform2(size: 10);
            var background = new Renderable(
                new Transform2(),
                new[] { plane }.ToList())
            {
                IsPortalable = false
            };
            var layer = new Layer(_scene);
            layer.Renderables.Add(background);
            _window.Layers.Add(layer);
        }

        public void Update(double timeDelta)
        {
            if (_window.ButtonDown(Key.W))
            {
                _camera.Velocity = Transform2.CreateVelocity(new Vector2(0, 1f));
            }
            else if (_window.ButtonDown(Key.S))
            {
                _camera.Velocity = Transform2.CreateVelocity(new Vector2(0, -1f));
            }
            else if (_window.ButtonDown(Key.A))
            {
                _camera.Velocity = Transform2.CreateVelocity(new Vector2(-1f, 0));
            }
            else if (_window.ButtonDown(Key.D))
            {
                _camera.Velocity = Transform2.CreateVelocity(new Vector2(1f, 0));
            }
            else
            {
                _camera.Velocity = Transform2.CreateVelocity();
            }

            _scene.Step();
        }
    }
}

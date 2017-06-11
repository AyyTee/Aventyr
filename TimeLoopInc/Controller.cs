using Game;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Controller : IUpdateable
    {
        readonly IVirtualWindow _window;
        Scene _scene;
        SceneRender _sceneRender;
        List<Input> _input = new List<Input>();
        int _updatesSinceLastStep = 0;
        int _updatesPerAnimation = 5;

        public Controller(IVirtualWindow window)
        {
            _window = window;
            Initialize();
        }

        public void Render(double timeDelta)
        {
            _sceneRender.Render(_window, _updatesSinceLastStep, _updatesPerAnimation);

            var gui = new Layer
            {
                DepthTest = false,
                Camera = new HudCamera2(_window.CanvasSize)
            };
            DrawTimeline(gui);
            gui.DrawText(_window.Fonts.Inconsolata, new Vector2(0, _window.CanvasSize.Y), "Time: " + _scene.CurrentInstant.Time.ToString());
            _window.Layers.Add(gui);
        }

        public void Initialize()
        {
            var Walls = new HashSet<Vector2i>()
            {
                new Vector2i(1, 1),
                new Vector2i(1, 2),
                new Vector2i(1, 4),
            };
            var player = new Player(new Transform2i(), 0);

            var portal0 = new TimePortal(new Vector2i(2, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-3, 2), GridAngle.Down);
            var portal2 = new TimePortal(new Vector2i(2, 2), GridAngle.Right);
            var portal3 = new TimePortal(new Vector2i(-3, 0), GridAngle.Left);
            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(10);

            portal2.SetLinked(portal3);
            portal2.SetTimeOffset(0);

            var Portals = new[]
            {
                portal0,
                portal1,
                portal2,
                portal3
            };

            var blocks = new[] {
                new Block(new Transform2i(new Vector2i(2, 0)), 0, 1),
                new Block(new Transform2i(new Vector2i(2, 1)), 1, 1),
                new Block(new Transform2i(new Vector2i(2, 2)), 2, 1),
            };

            _scene = new Scene(Walls, Portals, player, blocks);
            _sceneRender = new SceneRender(_scene);
        }

        public void Update(double timeDelta)
        {
            _updatesSinceLastStep++;

            _sceneRender.Update(_window);

            if (_window.ButtonPress(Key.BackSpace))
            {
                if (_input.Count > 0)
                {
                    Initialize();
                    _input.RemoveAt(_input.Count - 1);
                    foreach (var input in _input)
                    {
                        _scene.Step(input);
                    }
                }
            }
            else if (_updatesSinceLastStep >= _updatesPerAnimation)
            {
                var input = Input.CreateFromKeyboard(_window);
                if (input != null)
                {
                    _input.Add(input);
                    _scene.Step(input);
                    _updatesSinceLastStep = 0;
                }
            }
        }

        void DrawTimeline(IRenderLayer layer)
        {
            Vector2 topLeft = new Vector2(50, 100);
            Vector2 bottomRight = new Vector2(_window.CanvasSize.X - 50, 50);
            layer.DrawRectangle(topLeft, bottomRight, new Color4(0.8f, 0.8f, 0.8f, 0.8f));
        }
    }
}
 
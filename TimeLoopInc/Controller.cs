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
        TimelineRender _timelineRender;
        List<Input> _input = new List<Input>();
        int _updatesSinceLastStep = 0;
        int _updatesPerAnimation => _window.ButtonDown(KeyBoth.Control) ? 50 : 5;
        RollingAverage _fpsCounter = new RollingAverage(60, 0);

        public Controller(IVirtualWindow window)
        {
            _window = window;
            Initialize();
        }

        public void Initialize()
        {
            var walls = new HashSet<Vector2i>
            {
            };
            var player = new Player(new Transform2i(), 0);

            var portal0 = new TimePortal(new Vector2i(1, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-1, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(10);

            var Portals = new[]
            {
                portal0,
                portal1,
            };

            var entities = new[] {
                (IGridEntity)player,
                new Block(new Transform2i(new Vector2i(1, 0)), 0),
            };

            _scene = new Scene(walls, Portals, entities);
            _sceneRender = new SceneRender(_window, _scene);
            _timelineRender = new TimelineRender(_scene, _window.Fonts.Inconsolata);
            _timelineRender.Selected = player;
        }

        public void Render(double timeDelta)
        {
            var animationT = MathHelper.Clamp(_updatesSinceLastStep / (float)_updatesPerAnimation, 0, 1);

            _window.Layers.Clear();
            var worldLayer = _sceneRender.Render(animationT);
            _window.Layers.Add(worldLayer);

            var gui = new Layer
            {
                DepthTest = false,
                Camera = new HudCamera2(_window.CanvasSize)
            };
            _timelineRender.Render(gui, new Vector2(50, _window.CanvasSize.Y - 150), new Vector2(_window.CanvasSize.X - 100, 140), _window.DpiScale, animationT);
            gui.Renderables.Add(Draw.Text(_window.Fonts.Inconsolata, new Vector2(0, 0), "Time: " + _scene.CurrentTime.ToString()));
            gui.Renderables.Add(Draw.Text(_window.Fonts.Inconsolata, new Vector2(0, 30), _sceneRender.GetMouseGrid().ToString()));
            _fpsCounter.Enqueue((float)timeDelta);
            gui.Renderables.Add(Draw.Text(
                _window.Fonts?.Inconsolata,
                new Vector2(0, 80),
                $"FPS\nAvg { (1 / _fpsCounter.GetAverage()).ToString("00.00") }\nMin { (1 / _fpsCounter.Queue.Max()).ToString("00.00") }\n{_window.MousePosition}"));
            _window.Layers.Add(gui);
        }

        public void Update(double timeDelta)
        {
            _updatesSinceLastStep++;

            _sceneRender.Update(_window);
            _timelineRender.Update(timeDelta);
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

            if (_window.ButtonPress(MouseButton.Left))
            {
                var pos = _sceneRender.GetMouseGrid();
                _timelineRender.Selected = _scene.CurrentInstant.Entities.Keys
                    .FirstOrDefault(item => _scene.CurrentInstant.Entities[item].Transform.Position == (Vector2i)pos);
            }
        }
    }
}

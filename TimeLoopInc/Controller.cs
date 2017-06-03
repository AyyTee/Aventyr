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
            gui.DrawText(_window.Fonts.Inconsolata, new Vector2(0, 0), _scene.State.CurrentInstant.Time.ToString());
            _window.Layers.Add(gui);
        }

        public void Initialize()
        {
            _scene = new Scene();
            _sceneRender = new SceneRender(_scene);
        }

        public void Update(double timeDelta)
        {
            _updatesSinceLastStep++;

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
 
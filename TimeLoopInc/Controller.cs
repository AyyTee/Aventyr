using Game;
using Game.Common;
using Game.Rendering;
using OpenTK;
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
        Scene scene = new Scene();
        List<Input> _input = new List<Input>();
        int _updatesSinceLastStep = 0;
        int _updatesPerAnimation = 5;

        public Controller(IVirtualWindow window)
        {
            _window = window;
        }

        public void Render(double timeDelta)
        {
            float t = MathHelper.Clamp(_updatesSinceLastStep / (float)_updatesPerAnimation, 0, 1);

            _window.Layers.Clear();

            var worldLayer = new Layer();

            var state = scene.State;

            foreach (var player in state.Entities.Keys.OfType<Player>())
            {
                var posPrev = (Vector2)state.Entities[player].PreviousPosition;
                var pos = (Vector2)state.Entities[player].Position;
                worldLayer.Renderables.Add(new Renderable(posPrev.Lerp(pos, t)));
            }
            foreach (var block in state.Entities.Keys.OfType<Block>())
            {
                var posPrev = (Vector2)state.Entities[block].PreviousPosition;
                var pos = (Vector2)state.Entities[block].Position;
                worldLayer.Renderables.Add(new Renderable(posPrev.Lerp(pos, t)) { Color = new Vector4(0.5f, 1f, 0.8f, 1f) });
            }
            foreach (var portal in scene.Portals)
            {
                worldLayer.Renderables.Add(new Renderable(portal.Position) { Color = new Vector4(0.6f, 0.8f, 0.8f, 1f) });
            }
            foreach (var wall in scene.Walls)
            {
                worldLayer.Renderables.Add(new Renderable(wall) { Color = new Vector4(0.8f, 1f, 0.5f, 1f) });
            }

            worldLayer.Camera = new HudCamera2(_window.CanvasSize / 50);

            var gui = new Layer();
            gui.DrawText(_window.Fonts.Inconsolata, new Vector2(), scene.Time.ToString());
            gui.Camera = new HudCamera2(_window.CanvasSize);
            _window.Layers.Add(worldLayer);
            _window.Layers.Add(gui);
        }

        public void Update(double timeDelta)
        {
            _updatesSinceLastStep++;

            if (_window.ButtonPress(Key.BackSpace))
            {
                if (_input.Count > 0)
                {
                    scene = new Scene();
                    _input.RemoveAt(_input.Count - 1);
                    foreach (var input in _input)
                    {
                        scene.Step(input);
                    }
                }
            }
            else if (_updatesSinceLastStep >= _updatesPerAnimation)
            {
                var input = Input.CreateFromKeyboard(_window);
                if (input != null)
                {
                    _input.Add(input);
                    scene.Step(input);
                    _updatesSinceLastStep = 0;
                }
            }
        }
    }
}

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
        Scene scene = new Scene();
        List<Input> _input = new List<Input>();
        int _updatesSinceLastStep = 0;
        int _updatesPerAnimation = 5;
        Model _grid;

        public Controller(IVirtualWindow window)
        {
            _window = window;

            _grid = ModelFactory.CreateGrid(new Vector2i(20, 20), Vector2.One, Color4.HotPink, Color4.LightPink, new Vector3(-10,-10,-2));
        }

        public void Render(double timeDelta)
        {
            float t = MathHelper.Clamp(_updatesSinceLastStep / (float)_updatesPerAnimation, 0, 1);

            _window.Layers.Clear();

            var worldLayer = new Layer();

            var state = scene.State;

            foreach (var gridEntity in state.Entities.Keys.OfType<IGridEntity>())
            {
                var transform = GridEntityWorldPosition(gridEntity, t);

                Renderable renderable = null;
                switch (gridEntity)
                {
                    case Player p:
                        {
                            var model = ModelFactory.CreatePlane(Vector2.One, new Vector3(-0.5f));
                            model.SetColor(Color4.Black);

                            renderable = new Renderable(transform);
                            renderable.Models.Add(model);
                            break;
                        }
                        
                    case Block b:
                        {
                            var model = ModelFactory.CreatePlane(Vector2.One * b.Size, new Vector3(-0.5f));
                            model.SetColor(new Color4(0.5f, 1f, 0.8f, 1f));

                            renderable = new Renderable(transform);
                            renderable.Models.Add(model);
                            break;
                        }
                }

                worldLayer.Renderables.Add(renderable);

            }
            foreach (var portal in scene.Portals)
            {
                worldLayer.Renderables.Add(new Square(portal.Position) { Color = new Color4(0.6f, 0.8f, 0.8f, 1f) });
                worldLayer.Portals.Add(portal);
            }
            foreach (var wall in scene.Walls)
            {
                worldLayer.Renderables.Add(new Square(wall) { Color = new Color4(0.8f, 1f, 0.5f, 1f) });
            }

            worldLayer.Renderables.Add(new Renderable() { Models = new List<Model> { _grid }, IsPortalable = false });

            var cameraTransform = GridEntityWorldPosition(state.CurrentPlayer, t).SetSize(25);
            var worldCamera = new GridCamera(cameraTransform, (float)_window.CanvasSize.XRatio);
            worldLayer.Camera = worldCamera;

            var gui = new Layer()
            {
                DepthTest = false,
                Camera = new HudCamera2(_window.CanvasSize)
            };
            DrawTimeline(gui);
            gui.DrawText(_window.Fonts.Inconsolata, new Vector2(0, 0), scene.State.Time.ToString());
            
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

        void DrawTimeline(IRenderLayer layer)
        {
            Vector2 topLeft = new Vector2(50, _window.CanvasSize.Y - 100);
            Vector2 bottomRight = new Vector2(_window.CanvasSize.X - 50, _window.CanvasSize.Y - 50);
            layer.DrawRectangle(topLeft, bottomRight, new Color4(0.8f, 0.8f, 0.8f, 0.8f));
        }

        Transform2 GridEntityWorldPosition(IGridEntity gridEntity, float t)
        {
            var offset = Vector2d.One / 2;
            var velocity = (Vector2)scene.State.Entities[gridEntity].PreviousVelocity;
            var transform = scene.State.Entities[gridEntity].Transform.ToTransform2d();
            transform = transform.SetPosition(transform.Position + offset);
            var result = Ray.RayCast((Transform2)transform, new Transform2(-velocity * (1 - t)), scene.Portals, new Ray.Settings());

            return result.WorldTransform;
        }
    }
}
 
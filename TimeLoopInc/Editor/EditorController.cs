using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;
using Ui;

namespace TimeLoopInc.Editor
{
    public class EditorController
    {
        enum ToolType { Player, Wall, Portal, Link, Exit }

        readonly IVirtualWindow _window;
        readonly UiController _menu;
        readonly GridCamera _camera;
        readonly Controller _controller;
        SceneBuilder Scene => _sceneChanges.Last();
        Vector2i _mousePosition;
        List<SceneBuilder> _sceneChanges = new List<SceneBuilder>();
        ToolType _tool = ToolType.Wall;

        public EditorController(IVirtualWindow window, Controller controller)
        {
            _sceneChanges.Add(new SceneBuilder());

            _window = window;
            _controller = controller;
            _menu = new UiController(_window)
            {
                new Button(new Transform2(new Vector2(10, 10)), new Vector2(200, 90))
                {
                    new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(10, 10), "Save As..."))
                },
				new Button(new Transform2(new Vector2(10, 110)), new Vector2(200, 90))
				{
					new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(10, 10), "Play"))
				}
            };
            _camera = new GridCamera(new Transform2(), (float)_window.CanvasSize.XRatio);
            _camera.WorldTransform = _camera.WorldTransform.WithSize(10);
        }

        public void Update()
        {
            _mousePosition = (Vector2i)_window.MouseWorldPos(_camera).Floor(Vector2.One);

            _menu.Update(1);
            if (_menu.Hover == null)
            {
                if (_window.ButtonPress(MouseButton.Left))
                {
                    ApplyChanges(Scene.With(Scene.Walls.Add(_mousePosition)));
                }
            }

            if (_window.ButtonDown(KeyBoth.Control) && _window.ButtonPress(Key.Z))
            {
                if (_sceneChanges.Count > 1)
                {
                    _sceneChanges.RemoveAt(_sceneChanges.Count - 1);    
                }
            }
        }

        void ApplyChanges(SceneBuilder newScene)
        {
            _sceneChanges.Add(newScene);
        }

        public void Render()
        {
            var scene = Scene.CreateScene();

            var layer = new Layer
            {
                Camera = _camera,
                Renderables = SceneRender.RenderInstant(scene, scene.GetSceneInstant(0), 1, scene.Portals)
                    .Cast<IRenderable>()
                    .ToList()
            };
            _window.Layers.Add(layer);

            var gui = _menu.Render();
            gui.Renderables.Add(Draw.Text(_window.Fonts.Inconsolata, new Vector2(0, 130), _mousePosition.ToString()));
            _window.Layers.Add(gui);
		}
    }
}

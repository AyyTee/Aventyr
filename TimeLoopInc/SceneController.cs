using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game;
using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;

namespace TimeLoopInc
{
    public class SceneController
    {
        readonly IVirtualWindow _window;
        TimelineRender _timelineRender;
        public ImmutableList<IInput> Input { get; private set; } = new List<IInput>().ToImmutableList();
        List<Scene> _levels = new List<Scene>();
        int _currentLevel;
        Scene _scene;
        SceneRender _sceneRender;
        int _updatesSinceLastStep;
        int _updatesPerAnimation => _window.ButtonDown(KeyBoth.Control) ? 50 : 5;
        RollingAverage _fpsCounter = new RollingAverage(60, 0);

        public SceneController(IVirtualWindow window, IEnumerable<Scene> levels)
        {
            _window = window;
            _levels = levels.ToList();
            Initialize();
        }

        public void Initialize()
        {
            _scene = _levels[_currentLevel].DeepClone();
            _sceneRender = new SceneRender(_window, _scene);
            _timelineRender = new TimelineRender(_scene, _window.Fonts.LatoRegular());
            _timelineRender.Selected = _scene.CurrentPlayer;
        }

        public void SetInput(IEnumerable<IInput> newInput)
        {
            Input = newInput.ToImmutableList();

            var minTime = _timelineRender.MinTime;
            var maxTime = _timelineRender.MaxTime;
            var minRow = _timelineRender.MinRow;
            var maxRow = _timelineRender.MaxRow;
            Initialize();
            _timelineRender.MinTime = minTime;
            _timelineRender.MaxTime = maxTime;
            _timelineRender.MinRow = minRow;
            _timelineRender.MaxRow = maxRow;

            foreach (var input in Input)
            {
                if (input is MoveInput moveInput)
                {
                    _scene.Step(moveInput);
                }
                else if (input is SelectInput selectInput)
                {
                    SelectGrid(selectInput);
                }
            }

            _updatesSinceLastStep = _updatesPerAnimation;
        }

        public void Update(double timeDelta)
        {
            _updatesSinceLastStep++;

            _sceneRender.Update(_window);
            _timelineRender.Update(timeDelta);
            if (_window.ButtonPress(Key.BackSpace))
            {
                Undo();
            }
            else if (_updatesSinceLastStep >= _updatesPerAnimation)
            {
                if (_scene.IsCompleted())
                {
                    if (_currentLevel + 1 < _levels.Count)
                    {
                        _currentLevel++;
                        Input = Input.Clear();
                        Initialize();
                    }
                }
                else
                {
                    var input = MoveInput.FromKeyboard(_window);
                    if (input != null)
                    {
                        Input = Input.Add(input);
                        _scene.Step(input);
                        _updatesSinceLastStep = 0;
                    }
                }
            }

            if (_window.ButtonPress(MouseButton.Left))
            {
                var pos = (Vector2i)_sceneRender.GetMouseGrid();
                var input = new SelectInput(pos, _scene.CurrentTime);
                Input = Input.Add(input);
                SelectGrid(input);
            }
        }

        public void Render(double timeDelta)
        {
            var animationT = MathHelper.Clamp(_updatesSinceLastStep / (float)_updatesPerAnimation, 0, 1);

            var worldLayer = _sceneRender.Render(animationT);
            _window.Layers.Add(worldLayer);

            var gui = new Layer
            {
                DepthTest = false,
                Camera = new HudCamera2(_window.CanvasSize)
            };
            _timelineRender.Render(gui, new Vector2(50, _window.CanvasSize.Y - 150), new Vector2(_window.CanvasSize.X - 100, 140), _window.DpiScale, animationT);
            gui.Renderables.Add(Draw.Text(_window.Fonts.LatoRegular(), new Vector2(0, 0), "Time: " + _scene.CurrentTime.ToString()));
            gui.Renderables.Add(Draw.Text(_window.Fonts.LatoRegular(), new Vector2(0, 30), _sceneRender.GetMouseGrid().ToString()));
            _fpsCounter.Enqueue((float)timeDelta);
            gui.Renderables.Add(Draw.Text(
                _window.Fonts?.LatoRegular(),
                new Vector2(0, 80),
                $"FPS\nAvg { (1 / _fpsCounter.GetAverage()).ToString("00.00") }\nMin { (1 / _fpsCounter.Queue.Max()).ToString("00.00") }\n{_window.MousePosition}"));
            _window.Layers.Add(gui);
        }

        void SelectGrid(SelectInput input)
        {
            var entities = _scene.GetSceneInstant(_scene.CurrentTime).Entities;
            _timelineRender.Selected = entities.Keys
                .FirstOrDefault(item => entities[item].Transform.Position == input.Position);
        }

        void Undo()
        {
            if (Input.Count(item => !(item is SelectInput)) > 0)
            {
                var input = Input.ToList();
                while (input.LastOrDefault() is SelectInput)
                {
                    input.RemoveAt(input.Count - 1);
                }
                input.RemoveAt(input.Count - 1);
                SetInput(input);
            }
        }
    }
}

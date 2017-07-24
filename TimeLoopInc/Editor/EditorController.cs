using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Ui;

namespace TimeLoopInc.Editor
{
    public class EditorController
    {
        enum ToolType { Player, Block, Wall, Portal, Link, Exit }

        public static string LevelPath => "Levels";

        readonly IVirtualWindow _window;
        readonly UiController _menu;
        readonly GridCamera _camera;
        readonly Controller _controller;
        readonly PortalTool _portalTool;
        SceneController _sceneController;
        SceneBuilder Scene => _sceneChanges.Last();
        bool _isPlaying => _sceneController != null;
        Vector2 _mousePosition;
        Vector2i _mouseGridPos => (Vector2i)_mousePosition.Floor(Vector2.One);
        List<SceneBuilder> _sceneChanges = new List<SceneBuilder>();
        ToolType _tool = ToolType.Wall;

        public EditorController(IVirtualWindow window, Controller controller)
        {
            _sceneChanges.Add(new SceneBuilder());

            _window = window;
            _controller = controller;

            _portalTool = new PortalTool(_window);

            _menu = new UiController(_window);

            _menu.Root = new Frame(out Frame rootFrame)
            {
                new Frame(out Frame editor, new Transform2())
                {
                    new Button(out _, new Transform2(new Vector2(10, 10)), new Vector2(200, 90), Save)
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(10, 10), "Save As..."))
                    },
                    new Button(out _, new Transform2(new Vector2(10, 110)), new Vector2(200, 90), Load)
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(10, 10), "Load"))
                    },
                    new Button(out Button playButton, new Transform2(new Vector2(10, 210)), new Vector2(200, 90))
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(10, 10), "Play"))
                    }
                },
                new Frame(out Frame endGame, new Transform2(), true)
                {
                    new Button(out Button returnButton, new Transform2(new Vector2(10, 10)), new Vector2(200, 90))
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(10, 10), "Return to editor"))
                    },
                    new Button(out Button restart, new Transform2(new Vector2(10, 110)), new Vector2(200, 90))
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(10, 10), "Restart"))
                    }
                }
            };

            playButton.OnClick += () =>
            {
                if (Scene.Entities.OfType<Player>().Any())
                {
                    editor.Hidden = true;
                    endGame.Hidden = false;
                    _sceneController = new SceneController(window, new[] { Scene.CreateScene() });
                }
            };

            returnButton.OnClick += () =>
            {
                editor.Hidden = false;
                endGame.Hidden = true;
                _sceneController = null;
            };

            restart.OnClick += () =>
            {
                _sceneController.SetInput(_sceneController.Input.Clear());
            };

            _camera = new GridCamera(new Transform2(), (float)_window.CanvasSize.XRatio);
            _camera.WorldTransform = _camera.WorldTransform.WithSize(10);
        }

        void Save()
        {
            var filepath = Path.Combine(LevelPath, "Saved.xml");
            Directory.CreateDirectory(LevelPath);

            File.WriteAllText(filepath, Serializer.Serialize(Scene));
        }

        void Load()
        {
            var filepath = Path.Combine(LevelPath, "Saved.xml");
            if (File.Exists(filepath))
            {
                _sceneChanges = new[]
                {
                    Serializer.Deserialize<SceneBuilder>(File.ReadAllText(filepath))
                }.ToList();
            }
        }

        public void Update(double timeDelta)
        {
            _mousePosition = _window.MouseWorldPos(_camera);

            _menu.Update(1);
            if (_isPlaying)
            {
                _sceneController.Update(timeDelta);
            }
            else
            {
                if (_menu.Hover == null)
                {
                    switch (_tool)
                    {
                        case ToolType.Wall:
                            if (_window.ButtonPress(MouseButton.Left))
                            {
                                var walls = Scene.Walls.Add(_mouseGridPos);
                                var links = GetPortals(
                                    portal => PortalValidSides(portal.Position, walls).Any(),
                                    Scene.Links);
                                var newScene = Scene.With(walls, links: links);
                                ApplyChanges(newScene);
                            }
                            break;
                        case ToolType.Player:
                            if (_window.ButtonPress(MouseButton.Left))
                            {
                                var entities = Scene.Entities
                                    .RemoveAll(item => item is Player || item.StartTransform.Position == _mouseGridPos)
                                    .Add(new Player(new Transform2i(_mouseGridPos), 0));
                                ApplyChanges(Scene.With(entities: entities));
                            }
                            break;
                        case ToolType.Block:
                            if (_window.ButtonPress(MouseButton.Left))
                            {
                                var entities = Scene.Entities
                                    .RemoveAll(item => item.StartTransform.Position == _mouseGridPos)
                                    .Add(new Block(new Transform2i(_mouseGridPos)));
                                ApplyChanges(Scene.With(entities: entities));
                            }
                            break;
                        case ToolType.Exit:
                            if (_window.ButtonPress(MouseButton.Left))
                            {
                                var exits = Scene.Exits.Add(_mouseGridPos);
                                ApplyChanges(Scene.With(exits: exits));
                            }
                            break;
                        case ToolType.Portal:
                            {
                                var newScene = _portalTool.Update(Scene, _camera);
                                if (newScene != null)
                                {
                                    ApplyChanges(newScene);
                                }
                            }
                            break;
                        case ToolType.Link:
                            break;
                    }

                    if (_window.ButtonDown(KeyBoth.Control) && _window.ButtonPress(Key.Z))
                    {
                        if (_sceneChanges.Count > 1)
                        {
                            _sceneChanges.RemoveAt(_sceneChanges.Count - 1);
                        }
                    }

                    if (_window.ButtonDown(Key.Number1))
                    {
                        _tool = ToolType.Wall;
                    }
                    else if (_window.ButtonDown(Key.Number2))
                    {
                        _tool = ToolType.Player;
                    }
                    else if (_window.ButtonDown(Key.Number3))
                    {
                        _tool = ToolType.Block;
                    }
                    else if (_window.ButtonDown(Key.Number4))
                    {
                        _tool = ToolType.Exit;
                    }
                    else if (_window.ButtonDown(Key.Number5))
                    {
                        _tool = ToolType.Portal;
                    }
                    else if (_window.ButtonDown(Key.Number6))
                    {
                        _tool = ToolType.Link;
                    }
                }
            }
        }

        /// <summary>
        /// Returns portals colliding with the given portal. This method assumes portals are in valid positions.
        /// </summary>
        public static List<PortalBuilder> PortalCollisions(PortalBuilder portal, IEnumerable<PortalBuilder> portals)
        {
            var collisionPlaces = new[]
            {
                portal.Position + portal.Direction.Vector.PerpendicularLeft,
                portal.Position + portal.Direction.Vector.PerpendicularRight
            };

            return portals
                .Where(item =>
                    item.Position == portal.Position ||
                    (item.Direction.Vector == portal.Direction.Vector && collisionPlaces.Contains(item.Position)))
                .ToList();
        }

        /// <summary>
        /// Returns portals that meet some criteria while preserving the links they are in.
        /// </summary>
        public static ImmutableList<PortalLink> GetPortals(Func<PortalBuilder, bool> selector, IEnumerable<PortalLink> links)
        {
            return links
                .Select(item =>
                    new PortalLink(
                        item.Portals
                            .Where(linkPortal => selector(linkPortal))
                            .ToArray(),
                        item.TimeOffset))
                .Where(item => item.Portals.Length > 0)
                .ToImmutableList();
        }

        public static HashSet<GridAngle> PortalValidSides(Vector2i worldPosition, ISet<Vector2i> walls)
        {
            var validSides = new HashSet<GridAngle>();
            if (walls.Contains(worldPosition))
            {
                return validSides;
            }

            var up = worldPosition + new Vector2i(0, 1);
            var down = worldPosition + new Vector2i(0, -1);
            var left = worldPosition + new Vector2i(-1, 0);
            var right = worldPosition + new Vector2i(1, 0);

            var column = new[] { new Vector2i(0, -1), new Vector2i(), new Vector2i(0, 1) };
            var row = new[] { new Vector2i(-1, 0), new Vector2i(), new Vector2i(1, 0) };

            if (new[] { up, down }.All(item => !walls.Contains(item)))
            {
                if (column.Select(item => item + left).All(item => walls.Contains(item)))
                {
                    validSides.Add(GridAngle.Left);
                }
                if (column.Select(item => item + right).All(item => walls.Contains(item)))
                {
                    validSides.Add(GridAngle.Right);
                }
            }
            else if (new[] { left, right }.All(item => !walls.Contains(item)))
            {
                if (row.Select(item => item + up).All(item => walls.Contains(item)))
                {
                    validSides.Add(GridAngle.Up);
                }
                if (row.Select(item => item + down).All(item => walls.Contains(item)))
                {
                    validSides.Add(GridAngle.Down);
                }
            }
            return validSides;
        }

        public void ApplyChanges(SceneBuilder newScene)
        {
            _sceneChanges.Add(newScene);
        }

        public void Render(double timeDelta)
        {
            if (_isPlaying)
            {
                _sceneController.Render(timeDelta);
            }
            else
            {
                var scene = Scene.CreateScene();

                var layer = new Layer
                {
                    Camera = _camera,
                    Renderables = SceneRender.RenderInstant(scene, scene.GetSceneInstant(0), 1, scene.Portals, _window.Fonts.Inconsolata)
                        .Cast<IRenderable>()
                        .ToList()
                };

                if (_tool == ToolType.Portal)
                {
                    layer.Renderables.AddRange(_portalTool.Render(Scene, _camera));
                }

                _window.Layers.Add(layer);
            }

            var gui = _menu.Render();
            gui.Renderables.Add(Draw.Text(_window.Fonts.Inconsolata, new Vector2(0, 130), _mouseGridPos.ToString()));
            _window.Layers.Add(gui);
        }
    }
}

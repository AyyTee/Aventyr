﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using MoreLinq;
using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Ui;
using Game;
using static Ui.ElementEx;
using Ui.Elements;

namespace TimeLoopInc.Editor
{
    public class EditorController : IEditorController
    {
        enum ToolType { Player, Block, Wall, Portal, Link, Exit }

        public string SavePath => "Levels";

        public IVirtualWindow Window { get; }
        readonly UiController _menu;
        public GridCamera Camera { get; }
        readonly Controller _controller;
        public MouseButton PlaceButton { get; } = MouseButton.Right;
        public MouseButton SelectButton { get; } = MouseButton.Left;
        public Key DeleteButton { get; } = Key.Delete;
        SceneController _sceneController;
        public SceneBuilder Scene => _sceneChanges[_sceneChangeCurrent];
        bool _isPlaying => _sceneController != null;
        public string LevelName { get; set; } = "Level";
        Vector2 _mousePosition;
        Vector2i _mouseGridPos => (Vector2i)_mousePosition.Floor(Vector2.One);
        List<SceneBuilder> _sceneChanges = new List<SceneBuilder>();
        int _sceneChangeCurrent;
        int _saveChangeCurrent;
        ToolData _tool;
        readonly ImmutableArray<ToolData> _tools;

        private class ToolData
        {
            public ITool Tool { get; }
            public Hotkey Hotkey { get; }
            public string Name { get; }
            public string Hint { get; }

            public ToolData(string name, ITool tool, string hint, Hotkey hotkey = null)
            {
                DebugEx.Assert(tool != null);
                Name = name;
                Tool = tool;
                Hint = hint;
                Hotkey = hotkey;
            }
        }


        public EditorController(IVirtualWindow window, Controller controller)
        {
            _tools = new[]
            {
                new ToolData("Wall", new WallTool(this), "Left click to place.  Right click to remove.", new Hotkey(Key.Number1)),
                new ToolData("Exit", new ExitTool(this), "Left click to select.  Right click to place.  Delete to remove.", new Hotkey(Key.Number2)),
                new ToolData("Player", new PlayerTool(this), "Left click to select.  Right click to place.  Delete to remove.", new Hotkey(Key.Number3)),
                new ToolData("Block", new BlockTool(this), "Left click to select.  Right click to place.  Delete to remove.", new Hotkey(Key.Number4)),
                new ToolData("Portal", new PortalTool(this), "Left click to select.  Right click to place.  Delete to remove.  Shift+left links placed portal with previous.", new Hotkey(Key.Number5)),
                new ToolData("Portal Link", new LinkTool(this), "Click two portals to link them.", new Hotkey(Key.Number6)),
            }.ToImmutableArray();

            _sceneChanges.Add(new SceneBuilder());

            Window = window;
            _controller = controller;
            _tool = _tools[0];

            var root = new Frame()
            {
                new SaveDialogue(out SaveDialogue saveDialogue, this, Save),
                new LoadDialogue(out LoadDialogue loadDialogue, this, Load),
                new Frame(hidden: _ => _isPlaying)
                {
                    new StackFrame(spacing: _ => 20)
                    {
                        new StackFrame(thickness: _ => 200, spacing: _ => 5)
                        {
                            new Button(height: _ => 60, onClick: _ => NewLevel())
                            {
                                new TextBlock(AlignX(0.5f), AlignY(0.5f), _ => Window.Fonts.Inconsolata, _ => "New")
                            },
                            new Button(height: _ => 60, onClick: _ => saveDialogue.Show())
                            {
                                new TextBlock(AlignX(0.5f), AlignY(0.5f), _ => Window.Fonts.Inconsolata, _ => "Save As...")
                            },
                            new Button(height: _ => 60, onClick: _ => loadDialogue.Show())
                            {
                                new TextBlock(AlignX(0.5f), AlignY(0.5f), _ => Window.Fonts.Inconsolata, _ => "Load")
                            },
                            new Button(height: _ => 60, onClick: _ => Play())
                            {
                                new TextBlock(AlignX(0.5f), AlignY(0.5f), _ => Window.Fonts.Inconsolata, _ => "Play")
                            }
                        },
                        new StackFrame(thickness: _ => 120, spacing: _ => 2)
                        {
                            new DataTemplate<ToolData>(
                                () => new OrderedSet<ToolData>(_tools),
                                data => new Radio<ToolData>(
                                    height: ChildrenMaxY(),
                                    target: data,
                                    getValue: _ => _tool,
                                    setValue: _ => _tool = data)
                                {
                                    new TextBlock(
                                        AlignX(0.5f), 
                                        AlignY(0.5f), 
                                        _ => Window.Fonts.Inconsolata, 
                                        _ => data.Name, 
                                        args => (int)args.Parent.Width - 10,
                                        _ => 0.5f)
                                })
                        }
                    },
                    new TextBox(
                        _ => 220, _ => 10,
                        _ => 200, _ => 90,
                        _ => Window.Fonts.Inconsolata,
                        TimeOffsetGetText,
                        TimeOffsetSetText),
                    new TextBlock(x: _ => 5, y: AlignY(1), font: _ => Window.Fonts.Inconsolata, text: _ => _tool.Hint)
                },
                new StackFrame(thickness: _ => 90, spacing: _ => 5, hidden: _ => !_isPlaying, isVertical: false)
                {
                    new Button(width: _ => 200, onClick: _ => _sceneController = null)
                    {
                        new TextBlock(AlignX(0.5f), AlignY(0.5f),  _ => Window.Fonts.Inconsolata, _ => "Return to editor")
                    },
                    new Button(width: _ => 200, onClick: _ => _sceneController.SetInput(_sceneController.Input.Clear()))
                    {
                        new TextBlock(AlignX(0.5f), AlignY(0.5f),  _ => Window.Fonts.Inconsolata, _ => "Restart")
                    }
                },
                new TextBlock(AlignX(1), _ => 0, _ => Window.Fonts.Inconsolata, _ => LevelName + (_saveChangeCurrent == _sceneChangeCurrent ? "" : "*")),
            };

            _menu = new UiController(Window, root);

            Camera = new GridCamera(new Transform2(), (float)Window.CanvasSize.XRatio);
            Camera.WorldTransform = Camera.WorldTransform.WithSize(15);
        }

        void Play()
        {
            if (Scene.Entities.OfType<Player>().Any())
            {
                _sceneController = new SceneController(Window, new[] { Scene.CreateScene() });
            }
        }

        void NewLevel()
        {
            LevelName = "NewLevel";
            SetScene(new SceneBuilder());
        }

        void Load(SceneBuilder newScene)
        {
            SetScene(newScene);
        }

        void SetScene(SceneBuilder scene)
        {
            _sceneChanges = new[]
            {
                scene
            }.ToList();
            _sceneChangeCurrent = 0;
            _saveChangeCurrent = 0;
        }

        void Save(string saveName)
        {
            var filepath = Path.Combine(SavePath, saveName);
            Directory.CreateDirectory(SavePath);

            File.WriteAllText(filepath, Serializer.Serialize(Scene));
            _saveChangeCurrent = _sceneChangeCurrent;
        }

        string TimeOffsetGetText(ElementArgs args)
        {
            if (_tool.Tool is PortalTool portalTool)
            {
                var link = LinkTool.GetLink(LinkTool.SelectedPortal(Scene), Scene.Links);
                return link?.TimeOffset.ToString() ?? "";
            }
            return "";
        }

        void TimeOffsetSetText(string newText)
        {
            if (_tool.Tool is PortalTool portalTool)
            {
                var link = LinkTool.GetLink(LinkTool.SelectedPortal(Scene), Scene.Links);
                if (link != null)
                {
                    if (int.TryParse(newText, out int newValue))
                    {
                        var newLinks = Scene.Links.Remove(link).Add(new PortalLink(link.Portals, newValue));
                        ApplyChanges(Scene.With(links: newLinks));
                    }
                }
            }
        }

        public void Update(double timeDelta)
        {
            _mousePosition = Window.MouseWorldPos(Camera);

            _menu.Update(1);
            if (_isPlaying)
            {
                _sceneController.Update(timeDelta);
            }
            else
            {
                if (_menu.Hover == null)
                {
                    var v = MoveInput.FromKeyboard(Window)?.Direction?.Vector;
                    if (v != null)
                    {
                        Camera.WorldTransform = Camera.WorldTransform.AddPosition((Vector2)v * 3);
                    }

                    _tool.Tool.Update();

                    if (Window.ButtonDown(KeyBoth.Control))
                    {
                        if (Window.ButtonPress(Key.Y) || (Window.ButtonDown(KeyBoth.Shift) && Window.ButtonPress(Key.Z)))
                        {
                            RedoChanges();
                        }
                        else if (Window.ButtonPress(Key.Z))
                        {
                            UndoChanges();
                        }
                    }

                    _tools
                        .Where(item => Window.HotkeyPress(item.Hotkey))
                        .ForEach(item => _tool = item);
                }
            }
        }

        public static void DeleteAndSelect(IEditorController editor, Vector2i mouseGridPos)
        {
            if (editor.Window.ButtonPress(editor.DeleteButton))
            {
                editor.ApplyChanges(Remove(editor.Scene, mouseGridPos));
            }
            if (editor.Window.ButtonPress(editor.SelectButton))
            {
                editor.ApplyChanges(editor.Scene.With(mouseGridPos), true);
            }
        }

        public static SceneBuilder Remove(SceneBuilder scene, Vector2i v)
        {
            var walls = scene.Walls.Where(item => item != v).ToHashSet();
            return scene.With(
                walls, 
                scene.Exits.Where(item => item != v).ToHashSet(), 
                scene.Entities.Where(item => item.StartTransform.Position != v), 
                GetPortals(portal => portal.Position != v && PortalValidSides(portal.Position, walls).Any(), scene.Links));
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

        public void ApplyChanges(SceneBuilder newScene, bool undoSkip = false)
        {
            DebugEx.Assert(
                newScene.Links
                    .SelectMany(item => item.Portals)
                    .GroupBy(item => item)
                    .All(item => item.Count() == 1), 
                "There should not be any duplicate portals.");
            _sceneChangeCurrent++;
            _sceneChanges.RemoveRange(_sceneChangeCurrent, _sceneChanges.Count - _sceneChangeCurrent);
            _sceneChanges.Add(newScene);
        }

        public void UndoChanges()
        {
            if (_sceneChangeCurrent > 0)
            {
                _sceneChangeCurrent--;
            }
        }

        public void RedoChanges()
        {
            if (_sceneChangeCurrent + 1 < _sceneChanges.Count)
            {
                _sceneChangeCurrent++;
            }
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
                    Camera = Camera,
                    Renderables = SceneRender.RenderInstant(scene, scene.GetSceneInstant(0), 1, scene.Portals, Window.Fonts.Inconsolata)
                        .Cast<IRenderable>()
                        .ToList()
                };

                layer.Renderables.AddRange(_tool.Tool.Render());
                Window.Layers.Add(layer);
            }

            var gui = _menu.Render();
            //gui.Renderables.Add(Draw.Text(Window.Fonts.Inconsolata, new Vector2(0, 130), _mouseGridPos.ToString()));
            Window.Layers.Add(gui);
        }
    }
}

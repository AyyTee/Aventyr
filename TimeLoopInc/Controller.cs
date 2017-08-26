﻿using Game;
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
using System.Collections.Immutable;
using Ui;
using TimeLoopInc.Editor;
using Ui.Elements;

namespace TimeLoopInc
{
    public class Controller : IUpdateable
    {
        enum MenuState { Main, InGame, Editor }

        MenuState _menuState = MenuState.Main;

        readonly IVirtualWindow _window;
        UiController _menu;
        EditorController _editor;
        SceneController _sceneController;

        public Controller(IVirtualWindow window)
        {
            _window = window;

            var root = new Frame()
            {
                new StackFrame(thickness: _ => 200, spacing: _ => 5)
                {
                    new Button(height: _ => 80, onClick: StartGame)
                    {
                        new TextBlock(ElementEx.AlignX(0.5f), ElementEx.AlignY(0.5f), _ => _window.Fonts.Inconsolata, _ => "Start Game")
                    },
                    new Button(height: _ => 80, onClick: StartLevelEditor)
                    {
                        new TextBlock(ElementEx.AlignX(0.5f), ElementEx.AlignY(0.5f), _ => _window.Fonts.Inconsolata, _ => "Level Editor")
                    },
                    new Button(height: _ => 80, onClick: _ => _window.Exit())
                    {
                        new TextBlock(ElementEx.AlignX(0.5f), ElementEx.AlignY(0.5f), _ => _window.Fonts.Inconsolata, _ => "Exit")
                    }
                }
            };
            _menu = new UiController(_window, root);

            _editor = new EditorController(_window, this);
        }

        public void StartGame(ClickArgs args)
        {
            var levels = new[]
            {
                CreateLevel0(),
                CreateLevel1()
            };
            _sceneController = new SceneController(_window, levels);
            _menuState = MenuState.InGame;
        }

        public void StartLevelEditor(ClickArgs args)
        {
            _menuState = MenuState.Editor;
        }

        public Scene CreateLevel0()
        {
            var walls = new HashSet<Vector2i> { };
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
                new Block(new Transform2i(new Vector2i(1, 0))),
            };

            return new Scene(walls, Portals, entities, new HashSet<Vector2i> { new Vector2i(0, 2) });
        }

        public Scene CreateLevel1()
        {
            var walls = new HashSet<Vector2i>
            {
                new Vector2i(-1, 0)
            };
            var player = new Player(new Transform2i(), 0);

            var portal0 = new TimePortal(new Vector2i(1, 2), GridAngle.Up);
            var portal1 = new TimePortal(new Vector2i(-1, 0), GridAngle.Left);

            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(5);

            var Portals = new[]
            {
                portal0,
                portal1,
            };

            var entities = new[] {
                (IGridEntity)player,
                new Block(new Transform2i(new Vector2i(1, 0))),
            };

            return new Scene(walls, Portals, entities, new HashSet<Vector2i> { new Vector2i(0, 2) });
        }

        public void Render(double timeDelta)
        {
            _window.Layers.Clear();
            switch (_menuState)
            {
                case MenuState.Main:
                    _window.Layers.Add(_menu.Render());
                    break;
                case MenuState.Editor:
                    _editor.Render(timeDelta);
                    break;
                case MenuState.InGame:
                    _sceneController?.Render(timeDelta);
                    break;
            }
        }

        public void Update(double timeDelta)
        {
            switch (_menuState)
            {
                case MenuState.Main:
                    _menu.Update(1);
                    break;
                case MenuState.Editor:
                    _editor.Update(timeDelta);
                    break;
                case MenuState.InGame:
                    _sceneController?.Update(timeDelta);
                    break;
            }
        }
    }
}

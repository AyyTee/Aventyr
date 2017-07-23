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
            _menu = new UiController(_window)
            {
                Root = new Frame
                {
                    new Button(out _, new Transform2(new Vector2(10, 10)), new Vector2(200, 80), StartGame)
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(), "Start Game"))
                    },
                    new Button(out _, new Transform2(new Vector2(10, 100)), new Vector2(200, 80), StartLevelEditor)
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(), "Level Editor"))
                    },
                    new Button(out _, new Transform2(new Vector2(10, 190)), new Vector2(200, 80), _window.Exit)
                    {
                        new TextBlock(new TextEntity(_window.Fonts.Inconsolata, new Vector2(), "Exit"))
                    }
                }
            };


            _editor = new EditorController(_window, this);
        }

        public void StartGame()
        {
            var levels = new[]
			{
				CreateLevel0(),
				CreateLevel1()
			};
            _sceneController = new SceneController(_window, levels);
			_menuState = MenuState.InGame;
        }

        public void StartLevelEditor()
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

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
using Ui.Args;
using System.IO;
using static Ui.ElementEx;

namespace TimeLoopInc
{
    public enum MenuState { Main, LevelSelect, InGame, Editor }

    public class Controller : IUpdateable
    {
        MenuState _currentState = MenuState.Main;

        readonly IVirtualWindow _window;
        public UiController Menu;
        EditorController _editor;
        SceneController _sceneController;
        DateTime _menuChangeTime;

        public Controller(IVirtualWindow window)
        {
            _window = window;

            var menuButtons = new Style
            {
                new StyleElement<Button, float>(nameof(Button.Height), _ => 80),
            };

            var centerText = new Style
            {
                new StyleElement<TextBlock, float>(nameof(TextBlock.X), AlignX(0.5f)),
                new StyleElement<TextBlock, float>(nameof(TextBlock.Y), AlignY(0.5f))
            };

            _editor = new EditorController(_window, this);

            var root = new Frame()
            {
                new Frame(MenuTransition(MenuState.Main))
                {
                    new StackFrame(thickness: _ => 200, spacing: _ => 5, style: menuButtons.With(centerText))
                    {
                        new Button(onClick: SetMenuState(MenuState.LevelSelect))
                        {
                            new TextBlock(text: _ => "Start")
                        },
                        new Button(onClick: SetMenuState(MenuState.Editor))
                        {
                            new TextBlock(text: _ => "Level Editor")
                        },
                        new Button(onClick: _ => _window.Exit())
                        {
                            new TextBlock(text: _ => "Exit")
                        }
                    }
                },
                new EditorMenu(_editor, MenuTransition(MenuState.Editor)),
                LevelSelect.GetElements(MenuTransition(MenuState.LevelSelect))
            };

            Menu = new UiController(_window, root);
        }

        ElementFunc<float> MenuTransition(MenuState menuState)
        {
            return arg =>
            {
                var length = TimeSpan.FromSeconds(0.2);
                var t = MathHelper.Clamp(
                    (float)(arg.Controller.DateTime - _menuChangeTime).Div(length), 
                    0, 
                    1);
                return menuState == _currentState ?
                    arg.Parent.Width * (1 - t) :
                    arg.Parent.Width * t;
            };
        }

        public ElementAction<ClickArgs> SetMenuState(MenuState menuState)
        {
            return arg =>
            {
                _currentState = menuState;
                _menuChangeTime = arg.Controller.DateTime;
            };
        }

        public void Render(double timeDelta)
        {
            _window.Layers.Clear();
            switch (_currentState)
            {
                case MenuState.Editor:
                    _editor.Render(timeDelta);
                    break;
                case MenuState.InGame:
                    _sceneController?.Render(timeDelta);
                    break;
                case MenuState.LevelSelect:
                    break;
            }

            _window.Layers.Add(Menu.Render());
        }

        public void Update(double timeDelta)
        {
            switch (_currentState)
            {
                case MenuState.Editor:
                    _editor.Update(timeDelta);
                    break;
                case MenuState.InGame:
                    _sceneController?.Update(timeDelta);
                    break;
            }

            Menu.Update(1);
        }
    }
}

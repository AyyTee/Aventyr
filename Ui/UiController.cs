﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;
using System.Text;

namespace Ui
{
    public class UiController
    {
        public Frame Root { get; }
        readonly IVirtualWindow _window;
        ICamera2 _camera;
        List<UiWorldTransform> _flattenedUi = new List<UiWorldTransform>();
        public IElement Hover { get; private set; }
        public TextBox Selected { get; private set; }

        public UiController(IVirtualWindow window)
        {
            _window = window;
            Root = new Frame(width: _ => _window.CanvasSize.X, height: _ => _window.CanvasSize.Y);
        }

        public void Update(float uiScale)
        {
            _camera = new HudCamera2(_window.CanvasSize);
            var mousePos = _window.MouseWorldPos(_camera);

            UpdateElementArgs(Root);
            _flattenedUi = AllChildren();

            var hover = _flattenedUi
                .FirstOrDefault(item =>
                    item.Element.IsInside(
                        Vector2Ex.Transform(
                            mousePos,
                            item.WorldTransform.GetMatrix().Inverted())));
            Hover = hover?.Element;
            if (_window.ButtonPress(MouseButton.Left))
            {
                if (Hover != null)
                {
                    switch (Hover)
                    {
                        case Button button:
                            if (button.GetEnabled())
                            {
                                button.OnClick.Invoke();
                            }
                            break;
                        case TextBox textBox:
                            SetSelected(textBox);
                            break;
                    }
                }
                else
                {
                    SetSelected(null);
                }
            }

            if (_window.ButtonPress(Key.Enter))
            {
                SetSelected(null);
            }

            if (Selected != null)
            {
                var text = Selected.GetText();
                var newText = ApplyBackspaces(text + _window.KeyString);
                
                switch (Selected.InputType)
                {
                    case TextBox.Input.Text:
                        break;
                    case TextBox.Input.Numbers:
                        newText = newText
                            .Where(item => char.IsDigit(item) || item == '-')
                            .CharsToString();
                        break;
                }

                if (text != newText)
                {
                    Selected.SetText(newText);
                }
            }
        }

        public void SetSelected(TextBox selected)
        {
            Selected = selected;
        }

        public static string ApplyBackspaces(string stringWithBackspaces)
        {
            var newString = new StringBuilder();
            for (int i = 0; i < stringWithBackspaces.Length; i++)
            {
                if (stringWithBackspaces[i] == '\b' && newString.Length > 0)
                {
                    newString.Remove(newString.Length - 1, 1);
                    continue;
                }
                newString.Append(stringWithBackspaces[i]);
            }
            return newString.ToString();
        }

        void UpdateElementArgs(IElement root)
        {
            root.ElementArgs = new ElementArgs(null, root);
            _updateElementArgs(root);
        }

        void _updateElementArgs(IElement element)
        {
            foreach (var child in element)
            {
                child.ElementArgs = new ElementArgs(element, child);
                _updateElementArgs(child);
            }
        }

        List<UiWorldTransform> AllChildren()
        {
            var list = new List<UiWorldTransform>();
            _allChildren(Root, new Transform2(), list, null);
            return list;
        }

        void _allChildren(IElement element, Transform2 worldTransform, List<UiWorldTransform> list, IElement parent)
        {
            if (element.GetHidden())
            {
                return;
            }
            var transform = worldTransform.Transform(element.GetTransform());
            foreach (var child in element)
            {
                _allChildren(child, transform, list, element);
            }
            
            list.Add(new UiWorldTransform(element, transform));
        }

        public Layer Render()
        {
            return new Layer
            {
                DepthTest = false,
                Camera = _camera,
                Renderables = _flattenedUi
                    .Select(item => (IRenderable)new Renderable(item.WorldTransform, item.Element.GetModels()))
                    .Reverse()
                    .ToList()
            };
        }

        public bool IsInside(Vector2 localPoint) => true;

        public List<Model> GetModels() => new List<Model>();

        class UiWorldTransform
        {
            public IElement Element { get; }
            public Transform2 WorldTransform { get; }

            public UiWorldTransform(IElement element, Transform2 worldTransform)
            {
                DebugEx.Assert(element != null);
                DebugEx.Assert(worldTransform != null);
                Element = element;
                WorldTransform = worldTransform;
            }
        }
    }
}

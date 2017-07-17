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

namespace Ui
{
    public class UiController : IUiElement, IEnumerable<IUiElement>
    {
        public ImmutableList<IUiElement> Children { get; set; } = new List<IUiElement>().ToImmutableList();
        readonly IVirtualWindow _window;
        public ICamera2 Camera { get; set; }
        List<UiWorldTransform> _flattenedUi = new List<UiWorldTransform>();

        public Transform2 Transform => new Transform2();

        public UiController(IVirtualWindow window)
        {
            _window = window;
            Camera = new HudCamera2(_window.CanvasSize);
        }

        public void Update(float uiScale)
        {
            var mousePos = Camera.ScreenToWorld(_window.MousePosition, _window.CanvasSize);
            _flattenedUi = AllChildren();

            var buttonHover = _flattenedUi
                .FirstOrDefault(item =>
                    item.Element is Button &&
                    item.Element.IsInside(
                        Vector2Ex.Transform(
                            mousePos,
                            item.WorldTransform.GetMatrix().Inverted())));
            if (_window.ButtonPress(MouseButton.Left))
            {
                (buttonHover?.Element as Button)?.Click();
            }
        }

        List<UiWorldTransform> AllChildren()
        {
            var list = new List<UiWorldTransform>();
            _allChildren(this, new Transform2(), list);
            return list;
        }

        void _allChildren(IUiElement element, Transform2 worldTransform, List<UiWorldTransform> list)
        {
            foreach (var child in element.Children)
            {
                var transform = worldTransform.Transform(child.Transform);
                _allChildren(child, transform, list);
                list.Add(new UiWorldTransform(child, transform));
            }
        }

        public Layer Render()
        {
            return new Layer
            {
                DepthTest = false,
                Camera = Camera,
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
            public IUiElement Element { get; }
            public Transform2 WorldTransform { get; }

            public UiWorldTransform(IUiElement element, Transform2 worldTransform)
            {
                DebugEx.Assert(element != null);
                DebugEx.Assert(worldTransform != null);
                Element = element;
                WorldTransform = worldTransform;
            }
        }

        public IEnumerator<IUiElement> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void Add(IUiElement element)
        {
            Children = Children.Concat(new[] { element }).ToImmutableList();
        }
    }
}

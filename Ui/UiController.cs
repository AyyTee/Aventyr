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
    public class UiController
    {
        public Frame Root { get; set; } = new Frame();
        readonly IVirtualWindow _window;
        ICamera2 _camera;
        List<UiWorldTransform> _flattenedUi = new List<UiWorldTransform>();
        public IElement Hover { get; private set; }

        public Transform2 Transform => new Transform2();

        public UiController(IVirtualWindow window)
        {
            _window = window;
        }

        public void Update(float uiScale)
        {
            _camera = new HudCamera2(_window.CanvasSize);
            var mousePos = _window.MouseWorldPos(_camera);
            _flattenedUi = AllChildren();

            var buttonHover = _flattenedUi
                .FirstOrDefault(item =>
                    item.Element is Button &&
                    item.Element.IsInside(
                        Vector2Ex.Transform(
                            mousePos,
                            item.WorldTransform.GetMatrix().Inverted())));
            Hover = buttonHover?.Element;
            if (_window.ButtonPress(MouseButton.Left))
            {
                (buttonHover?.Element as Button)?.Click();
            }
        }

        List<UiWorldTransform> AllChildren()
        {
            var list = new List<UiWorldTransform>();
            _allChildren(Root, new Transform2(), list);
            return list;
        }

        void _allChildren(IElement element, Transform2 worldTransform, List<UiWorldTransform> list)
        {
            if (element.Hidden)
            {
                return;
            }
            var transform = worldTransform.Transform(element.Transform);
            foreach (var child in element)
            {
                _allChildren(child, transform, list);
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

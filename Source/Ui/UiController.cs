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
using OpenTK.Graphics;
using Ui.Elements;
using Ui.Args;

namespace Ui
{
    public class UiController : IUiController
    {
        public Frame Root { get; }
        readonly IVirtualWindow _window;
        ICamera2 _camera;
        List<UiWorldTransform> _flattenedUi = new List<UiWorldTransform>();
        public IHoverable Hovered { get; private set; }
        public ISelectable Selected { get; private set; }
        public (IClickable Element, DateTime Time) LastClick = (null, new DateTime());
        public DateTime DateTime { get; private set; }
        public TimeSpan DoubleClickSpeed { get; set; } = TimeSpan.FromSeconds(0.6);

        public Resources Fonts => _window.Resources;

        public UiController(IVirtualWindow window, Frame root)
        {
            _window = window;

            Root = root;
            Root.Style = DefaultStyle();
            Root._x = _ => 0;
            Root._y = _ => 0;
            Root._width = _ => _window.CanvasSize.X;
            Root._height = _ => _window.CanvasSize.Y;
        }

        public ImmutableDictionary<(Type, string), ElementFunc<object>> DefaultStyle()
        {
            return new Style
            {
                Button.DefaultStyle(this),
                Radio<object>.DefaultStyle(this),
                Rectangle.DefaultStyle(this),
                StackFrame.DefaultStyle(this),
                TextBlock.DefaultStyle(this),
                TextBox.DefaultStyle(this),
                Element.DefaultStyle(this),
            }.ToImmutable();
        }

        public void Update(float uiScale)
        {
            DateTime = DateTime.UtcNow;

            _camera = new HudCamera2(() => _window.CanvasSize);
            var mousePos = _window.MouseWorldPos(_camera);

            UpdateElementArgs(Root);
            _flattenedUi = AllChildren();

            DebugEx.Assert(
                _flattenedUi.Distinct().Count() == _flattenedUi.Count, 
                "Elements should not be used in multiple places.");

            var hover = _flattenedUi
                .FirstOrDefault(item =>
                    item.Element.IsInside(
                        Vector2Ex.Transform(
                            mousePos,
                            item.WorldTransform.GetMatrix().Inverted())));

            var hoverPrevious = Hovered;
            Hovered = hover?.Element as IHoverable;
            if (Hovered != hoverPrevious)
            {
                if (hoverPrevious is IHoverable hoverablePrevious)
                {
                    hoverablePrevious.OnHover(new HoverArgs(hoverablePrevious, false, this));
                }
                if (Hovered is IHoverable hoverable)
                {
                    hoverable.OnHover(new HoverArgs(hoverable, true, this));
                }
            }

            if (_window.ButtonPress(MouseButton.Left))
            {
                if (Hovered is IRadio radio)
                {
                    radio.SetValue();
                }
                if (Hovered is IClickable clickable)
                {
                    var isDoubleClick =
                        (DateTime - LastClick.Time < DoubleClickSpeed) &&
                        LastClick.Element == Hovered;

                    clickable.OnClick(new ClickArgs(Hovered, isDoubleClick, this));

                    LastClick = (clickable, DateTime);
                }
                if (Hovered is ISelectable selectable)
                {
                    SetSelected(selectable);
                }
                if (Hovered == null)
                {
                    SetSelected(null);
                }
            }

            if (_window.ButtonPress(Key.Enter))
            {
                SetSelected(null);
            }

            if (Selected is ITypeable typeable)
            {
                typeable.OnTyping(new TypeArgs(Selected, _window, this));
            }
        }

        public void SetSelected(ISelectable selected)
        {
            if (Selected != null)
            {
                Selected.OnSelect(new SelectArgs(Selected, false, this));
            }
            Selected = selected;
            if (Selected != null)
            {
                Selected.OnSelect(new SelectArgs(Selected, true, this));
            }
        }

        void UpdateElementArgs(Element root)
        {
            root.ElementArgs = new ElementArgs(null, root, this);
            _updateElementArgs(root);
        }

        void _updateElementArgs(Element element)
        {
            foreach (var child in element)
            {
                child.ElementArgs = new ElementArgs(element, child, this);
                _updateElementArgs(child);
            }
        }

        List<UiWorldTransform> AllChildren()
        {
            var list = new List<UiWorldTransform>();
            _allChildren(Root, new Transform2(), list);
            return list;
        }

        void _allChildren(Element element, Transform2 worldTransform, List<UiWorldTransform> list)
        {
            if (element.Hidden)
            {
                return;
            }
            var transform = worldTransform.Transform(element.GetTransform());
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
                    .Select(item =>
                    {
                        var element = item.Element;
                        var elementArgs = item.Element.ElementArgs;
                        return (IRenderable)new Renderable(
                            item.WorldTransform,
                            element.GetModels(new ModelArgs(elementArgs.Parent, elementArgs.Self, this)))
                        {
                            PixelAlign = element is TextBlock || element is TextBox
                        };
                    })
                    .Reverse()
                    .ToList()
            };
        }

        class UiWorldTransform
        {
            public Element Element { get; }
            public Transform2 WorldTransform { get; }

            public UiWorldTransform(Element element, Transform2 worldTransform)
            {
                DebugEx.Assert(element != null);
                DebugEx.Assert(worldTransform != null);
                Element = element;
                WorldTransform = worldTransform;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;

namespace TimeLoopInc
{
    public class Button : IRenderable
    {
        public delegate void Clicked();
        public event Clicked ClickedEvent;

        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get; set; }

        public Vector2 Size => BottomRight - TopLeft;

        public string Text { get; set; } = "";

        public bool Visible { get; set; } = true;

        public bool DrawOverPortals => false;

        public bool IsPortalable => true;

        public List<ClipPath> ClipPaths { get; set; } = new List<ClipPath>();

        public Transform2 WorldTransform => new Transform2((TopLeft + BottomRight) / 2);

        public Transform2 WorldVelocity => Transform2.CreateVelocity();

        public Button()
        {
        }

        public Button(Vector2 topLeft, Vector2 bottomRight)
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        public void Click()
        {
            ClickedEvent?.Invoke();
        }

        public List<Model> GetModels()
        {
            return new[] { ModelFactory.CreatePlane(Size, Color4.Black) }.ToList();
        }
    }
}

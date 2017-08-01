using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using OpenTK;
using Game.Rendering;
using OpenTK.Graphics;

namespace Ui
{
    public class TextBox : IElement
    {
        public enum Input { Text, Numbers }

        public Transform2 Transform { get; set; } = new Transform2();
        public bool Hidden { get; set; }
        public Vector2 Size { get; set; }
        public Font Font { get; }
        public string Text { get; set; } = "";
        public Input InputType { get; set; } = Input.Numbers;
        public int CursorStart { get; set; }
        public int CursorEnd { get; set; }

        public TextBox(Transform2 transform, Vector2 size, Font font, string text = "")
        {
            DebugEx.Assert(transform != null);
            DebugEx.Assert(size.X >= 0 && size.Y >= 0);
            Transform = transform;
            Size = size;
            Font = font;
            Text = text;
        }

        public TextBox(out TextBox id, Transform2 transform, Vector2 size, Font font, string text = "")
            : this(transform, size, font, text)
        {
            id = this;
        }

        public List<Model> GetModels()
        {
            var models = new List<Model>();
            var margin = new Vector2(2, 2);
            if (Size != new Vector2())
            {
                models.AddRange(Draw.Rectangle(new Vector2(), Size, Color4.Brown).GetModels());
                models.AddRange(Draw.Rectangle(margin, Size - margin, Color4.White).GetModels());
                models.AddRange(Draw.Text(Font, margin, Text, Color4.Black).GetModels());
            }
            return models;
        }

        public bool IsInside(Vector2 localPoint)
        {
            return MathEx.PointInRectangle(new Vector2(), Size, localPoint);
        }

        public IEnumerator<IElement> GetEnumerator() => new List<IElement>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

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
    public class TextBox : Element, IElement
    {
        public enum Input { Text, Numbers }

        public Font Font { get; }
        public Func<string> GetText { get; }
        public Action<string> SetText { get; }
        public string Text => GetText();
        public string DisplayText { get; set; } = "";
        public Input InputType { get; set; } = Input.Numbers;
        public int CursorStart { get; set; }
        public int CursorEnd { get; set; }
        public bool Selected { get; private set; }

        public TextBox(
            ElementFunc<Transform2> transform = null,
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            Font font = null, 
            Func<string> getText = null, 
            Action<string> setText = null,
            ElementFunc<bool> hidden = null)
            : base(transform, width, height, hidden)
        {
            Font = font;

            GetText = getText ?? (() => "");
            SetText = setText;
        }

        public TextBox(
            out TextBox id, 
            ElementFunc<Transform2> transform = null,
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            Font font = null, 
            Func<string> getText = null, 
            Action<string> setText = null,
            ElementFunc<bool> hidden = null)
            : this(transform, width, height, font, getText, setText, hidden)
        {
            id = this;
        }

        public void SetSelected(bool selected)
        {
            Selected = selected;
            if (selected)
            {
                DisplayText = GetText();
            }
            else if (!selected && DisplayText != GetText())
            {
                SetText?.Invoke(DisplayText);
            }
        }

        public override List<Model> GetModels()
        {
            var models = new List<Model>();
            var margin = new Vector2(2, 2);
            var size = this.GetSize();
            if (size != new Vector2())
            {
                models.AddRange(Draw.Rectangle(new Vector2(), size, Color4.Brown).GetModels());
                models.AddRange(Draw.Rectangle(margin, size - margin, Color4.White).GetModels());
                models.AddRange(Draw.Text(Font, margin, Selected ? DisplayText : GetText(), Color4.Black).GetModels());
            }
            return models;
        }

        public override bool IsInside(Vector2 localPoint)
        {
            return MathEx.PointInRectangle(new Vector2(), this.GetSize(), localPoint);
        }

        public IEnumerator<IElement> GetEnumerator() => new List<IElement>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

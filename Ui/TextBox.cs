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
    public class TextBox : Element, IElement, ISelectable
    {
        public enum Input { Text, Numbers }

        public ElementFunc<Font> FontFunc { get; }
        public ElementFunc<Color4> BackgroundColorFunc { get; }
        public ElementFunc<string> TextFunc { get; }
        public Action<string> SetText { get; }
        public int? CursorIndex { get; set; }

        [DetectLoop]
        public string Text => TextFunc(ElementArgs);
        [DetectLoop]
        public Font Font => FontFunc(ElementArgs);
        [DetectLoop]
        public Color4 BackgroundColor => BackgroundColorFunc(ElementArgs);

        public TextBox(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null,
            ElementFunc<Font> font = null,
            ElementFunc<string> getText = null, 
            Action<string> setText = null,
            ElementFunc<Color4> backgroundColor = null,
            ElementFunc<bool> hidden = null)
            : base(x, y, width, height, hidden)
        {
            FontFunc = font;

            var defaultText = "";
            TextFunc = getText ?? (_ => defaultText);
            SetText = setText ?? (newText => defaultText = newText);

            BackgroundColorFunc = backgroundColor ?? (_ => Color4.White);
        }

        public TextBox(
            out TextBox id,
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null,
            ElementFunc<Font> font = null,
            ElementFunc<string> getText = null, 
            Action<string> setText = null,
            ElementFunc<Color4> backgroundColor = null,
            ElementFunc<bool> hidden = null)
            : this(x, y, width, height, font, getText, setText, backgroundColor, hidden)
        {
            id = this;
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            var models = new List<Model>();
            var margin = new Vector2i(2, 2);
            var size = this.GetSize();
            if (size != new Vector2())
            {
                models.AddRange(Draw.Rectangle(new Vector2(), size, Color4.Brown).GetModels());
                models.AddRange(Draw.Rectangle((Vector2)margin, size - (Vector2)margin, BackgroundColor).GetModels());

                var settings = new Font.Settings();

                var font = Font;
                var text = Text;

                var textPos = new Vector2i(
                    margin.X + 5,
                    ((int)size.Y - font.GetSize(text, settings).Y) / 2);

                if (CursorIndex != null)
                {
                    var cursorPos = textPos + font.BaselinePosition(text, (int)CursorIndex, settings);

                    var cursor = Draw.Rectangle(
                        (Vector2)(cursorPos + new Vector2i(-1, -font.FontData.Common.Base)),
                        (Vector2)(cursorPos + new Vector2i(1, -font.FontData.Common.Base + font.FontData.Info.Size)), Color4.Black).GetModels();
                    models.AddRange(cursor);
                }

                var textModel = font.GetModel(text, Color4.Black);
                textModel.Transform.Position += new Vector3(
                    textPos.X,
                    textPos.Y, 
                    0); 
                models.Add(textModel);
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

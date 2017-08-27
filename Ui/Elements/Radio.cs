using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Models;
using Game.Rendering;
using OpenTK.Graphics;
using System.Collections.Immutable;

namespace Ui.Elements
{
    public class Radio<T> : Button, IRadio, IElement
    {
        internal ElementFunc<T> _value;
        internal Action<T> SetValueFunc { get; }
        public T Target { get; }
        public T Value => _value(ElementArgs);

        public bool Selected => Target.Equals(Value);

        public Radio(
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            OnClickHandler onClick = null,
            OnHoverHandler onHover = null,
            T target = default(T),
            ElementFunc<T> getValue = null,
            Action<T> setValue = null,
            ElementFunc<bool> enabled = null, 
            ElementFunc<bool> hidden = null,
            Style style = null)
            : base(x, y, width, height, onClick, onHover, enabled, hidden, style)
        {
            var internalValue = default(T);
            _value = getValue ?? (_ => internalValue);
            SetValueFunc = setValue ?? (value => internalValue = value);
            Target = target;
        }

        public Radio(
            out Button id,
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            OnClickHandler onClick = null,
            OnHoverHandler onHover = null,
            T value = default(T),
            ElementFunc<T> getValue = null,
            Action<T> setValue = null,
            ElementFunc<bool> enabled = null,
            ElementFunc<bool> hidden = null,
            Style style = null)
            : this(x, y, width, height, onClick, onHover, value, getValue, setValue, enabled, hidden, style)
        {
            id = this;
        }

        public void SetValue() => SetValueFunc(Target);

        public static new Style DefaultStyle(IUiController controller)
        {
            var type = typeof(Radio<>);
            return new Style
            {
            };
        }

        public override List<Model> GetModels(ModelArgs args)
        {
            var color = Enabled ?
                Selected ? Color4.DarkGray : Color4.Black :
                new Color4(0, 0, 0, 0.5f);
            return new[]
            {
                ModelFactory.CreatePlane(this.GetSize(), color),
            }.ToList();
        }
    }

    public interface IRadio
    {
        bool Selected { get; }

        void SetValue();
    }
}

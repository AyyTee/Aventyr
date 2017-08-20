using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Models;
using Game.Rendering;
using OpenTK.Graphics;

namespace Ui
{
    public class Radio<T> : Button, IRadio, IElement
    {
        internal ElementFunc<T> GetValueFunc { get; }
        internal Action<T> SetValueFunc { get; }
        public T Target { get; }
        [DetectLoop]
        public T Value => GetValueFunc(ElementArgs);

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
            ElementFunc<bool> enabled = null)
            : base(x, y, width, height, onClick, onHover, enabled)
        {
            var internalValue = default(T);
            GetValueFunc = getValue ?? (_ => internalValue);
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
            ElementFunc<bool> enabled = null)
            : this(x, y, width, height, onClick, onHover, value, getValue, setValue, enabled)
        {
            id = this;
        }

        public void SetValue() => SetValueFunc(Target);

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

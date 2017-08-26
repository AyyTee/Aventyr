using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game.Common;
using Game.Models;
using OpenTK;

namespace Ui
{
    public class Frame : NodeElement, IElement
    {
        public Frame(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            ElementFunc<bool> hidden = null,
            ImmutableDictionary<(Type, string), ElementFunc<object>> style = null)
            : base(x, y, width, height, hidden, style)
        {
        }

        public Frame(
            out Frame id, 
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            ElementFunc<bool> hidden = null,
            ImmutableDictionary<(Type, string), ElementFunc<object>> style = null)
            : this(x, y, width, height, hidden, style)
        {
            id = this;
        }
    }
}

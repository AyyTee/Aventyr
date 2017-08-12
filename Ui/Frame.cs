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
            ElementFunc<bool> hidden = null)
            : base(x, y, width, height, hidden)
        {
        }

        public Frame(
            out Frame id, 
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            ElementFunc<bool> hidden = null)
            : this(x, y, width, height, hidden)
        {
            id = this;
        }
    }
}

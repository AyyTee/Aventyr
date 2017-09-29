using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game.Common;
using Game.Models;
using OpenTK;

namespace Ui.Elements
{
    public class Frame : NodeElement
    {
        public Frame(
            ElementFunc<float> x = null, 
            ElementFunc<float> y = null, 
            ElementFunc<float> width = null, 
            ElementFunc<float> height = null, 
            ElementFunc<bool> hidden = null,
            Style style = null)
            : base(x, y, width, height, hidden, style)
        {
        }
    }
}

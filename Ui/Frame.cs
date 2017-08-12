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
        public Frame(ElementFunc<Transform2> transform = null, ElementFunc<float> width = null, ElementFunc<float> height = null, ElementFunc<bool> hidden = null)
            : base(transform, width, height, hidden)
        {
        }

        public Frame(out Frame id, ElementFunc<Transform2> transform = null, ElementFunc<float> width = null, ElementFunc<float> height = null, ElementFunc<bool> hidden = null)
            : this(transform, width, height, hidden)
        {
            id = this;
        }
    }
}

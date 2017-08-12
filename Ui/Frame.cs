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
        public Frame(Func<ElementArgs, Transform2> transform = null, Func<ElementArgs, float> width = null, Func<ElementArgs, float> height = null, Func<ElementArgs, bool> hidden = null)
            : base(transform, width, height, hidden)
        {
        }

        public Frame(out Frame id, Func<ElementArgs, Transform2> transform = null, Func<ElementArgs, float> width = null, Func<ElementArgs, float> height = null, Func<ElementArgs, bool> hidden = null)
            : this(transform, width, height, hidden)
        {
            id = this;
        }
    }
}

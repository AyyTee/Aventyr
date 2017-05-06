﻿using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public interface IPortalRenderable
    {
        Transform2 GetWorldTransform();
        IPortalRenderable Linked { get; }
        bool OneSided { get; }
    }
}
﻿using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc.Editor
{
    public interface ITool
    {
        void Update();
        List<IRenderable> Render();
    }
}

﻿using System;
using System.Collections.Generic;
using Game.Common;

namespace TimeLoopInc.Editor
{
    public class SceneBuilder
    {
        public HashSet<Vector2i> Walls { get; set; } = new HashSet<Vector2i>();
        public HashSet<Vector2i> Exits { get; set; } = new HashSet<Vector2i>();
        public List<IGridEntity> Entities { get; set; } = new List<IGridEntity>();
        public List<TimePortal> Portals { get; set; } = new List<TimePortal>();

        public Scene CreateScene()
        {
            return new Scene(Walls, Portals, Entities, Exits);
        }
    }
}

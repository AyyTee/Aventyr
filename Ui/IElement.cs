﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Game;
using Game.Common;
using Game.Models;
using OpenTK;

namespace Ui
{
    public interface IElement
    {
        ImmutableList<IElement> Children { get; }
        Transform2 Transform { get; }

        bool IsInside(Vector2 localPoint);
        List<Model> GetModels();
    }
}

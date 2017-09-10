using Game.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ModelGroup
    {
        public ImmutableArray<Model> Models { get; }

        public ModelGroup(ImmutableArray<Model> models)
        {
            Models = models;
        }
    }
}

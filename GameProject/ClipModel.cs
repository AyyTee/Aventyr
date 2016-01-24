using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ClipModel
    {
        readonly Line[] _clipLines;
        public Line[] ClipLines { get { return _clipLines; } }
        readonly Matrix4 _transform;
        public Matrix4 Transform { get { return _transform; } }
        public readonly Model Model;
        public readonly Entity Entity;

        public ClipModel(Entity entity, Model model, Line[] clipLines, Matrix4 transform)
        {
            Entity = entity;
            Model = model;
            _clipLines = clipLines;
            _transform = transform;
        }
    }
}

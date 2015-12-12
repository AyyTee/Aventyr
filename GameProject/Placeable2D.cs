
using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Game
{
    [Serializable]
    public abstract class Placeable2D
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        List<Placeable2D> _children = new List<Placeable2D>();
        public List<Placeable2D> ChildList { get { return new List<Placeable2D>(_children); } }
        public Placeable2D Parent { get; private set; }
        Transform2D _transform = new Transform2D();
        public virtual Transform2D Transform { get { return _transform; } }

        public Scene Scene { get; private set; }

        public Placeable2D()
        {
        }

        public Placeable2D(Scene scene)
        {
            Scene = scene;
            if (Scene != null)
            {
                Id = Scene.IdCount;
            }
        }

        public virtual void SetParent(Placeable2D parent)
        {
            if (parent != null)
            {
                parent._children.Remove(this);
            }
            Parent = parent;
            parent._children.Add(this);
            Debug.Assert(!ParentLoopExists(), "Cannot have cycles in Parent tree.");
        }

        public virtual Transform2D GetTransform()
        {
            return _transform.Copy();
        }

        public virtual void SetTransform(Transform2D transform)
        {
            _transform = transform.Copy();
        }

        public virtual Transform2D GetWorldTransform()
        {
            if (Parent != null)
            {
                return GetTransform().Transform(Parent.GetWorldTransform());
            }
            return GetTransform();
        }

        /// <summary>
        /// Returns true if there is a loop in the Parent dependencies.
        /// </summary>
        /// <returns></returns>
        private bool ParentLoopExists()
        {
            const int DONT_CARE = 0;
            Dictionary<Placeable2D, int> map = new Dictionary<Placeable2D, int>();
            Placeable2D child = this;
            while (child.Parent != null)
            {
                child = child.Parent;
                if (map.ContainsKey(child))
                {
                    return true;
                }
                map.Add(child, DONT_CARE);
            }
            return false;
        }
    }
}

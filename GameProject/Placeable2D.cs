
using FarseerPhysics.Dynamics;
using OpenTK;
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

        public Scene Scene { get; private set; }

        #region constructors
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
        #endregion

        public abstract Placeable2D DeepClone();

        public static void DeepClone(Placeable2D source, Placeable2D destination)
        {
            destination.Name = source.Name; 
            destination.RemoveChildren();
            foreach (Placeable2D p in source.ChildList)
            {
                destination._children.Add(p.DeepClone());
            }
        }

        public virtual void SetParent(Placeable2D parent)
        {
            if (Parent != null)
            {
                Parent._children.Remove(this);
            }
            Parent = parent;
            if (Parent != null)
            {
                parent._children.Add(this);
            }
            Debug.Assert(!ParentLoopExists(), "Cannot have cycles in Parent tree.");
        }

        public void RemoveChildren()
        {
            for (int i = 0; i < ChildList.Count; i++)
            {
                ChildList[i].SetParent(null);
            }
        }

        public virtual Transform2D GetTransform()
        {
            return _transform.Copy();
        }

        public virtual void SetTransform(Transform2D transform)
        {
            _transform = transform.Copy();
        }

        public virtual void SetPosition(Vector2 position)
        {
            Transform2D transform = GetTransform();
            transform.Position = position;
            SetTransform(transform);
        }

        public virtual void SetRotation(float rotation)
        {
            Transform2D transform = GetTransform();
            transform.Rotation = rotation;
            SetTransform(transform);
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

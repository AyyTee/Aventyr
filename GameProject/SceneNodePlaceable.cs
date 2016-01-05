using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class SceneNodePlaceable : SceneNode
    {
        Transform2D _transform = new Transform2D();

        public SceneNodePlaceable(Scene scene)
            : base (scene)
        {
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

        public virtual void SetScale(Vector2 scale)
        {
            Transform2D transform = GetTransform();
            transform.Scale = scale;
            SetTransform(transform);
        }

        public override Transform2D GetTransform()
        {
            return _transform.Copy();
        }
    }
}

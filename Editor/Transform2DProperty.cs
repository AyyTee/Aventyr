using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class Transform2DProperty
    {
        Transform2D _transform;

        [TypeConverter(typeof(PropertyGridConverter)), BrowsableAttribute(true)]
        public Vector2 Position { get { return _transform.Position; } set { _transform.Position = value; } }

        [BrowsableAttribute(true)]
        public float Rotation { get { return _transform.Rotation; } set { _transform.Rotation = value; } }

        [TypeConverter(typeof(PropertyGridConverter)), BrowsableAttribute(true)]
        public Vector2 Scale 
        { 
            get { return _transform.Scale; }
            set 
            { 
                if (value.X != 0 && value.Y != 0)
                {
                    _transform.Scale = value; 
                }
            }
        }

        public Transform2DProperty(Transform2D transform)
        {
            _transform = transform;
        }
    }
}

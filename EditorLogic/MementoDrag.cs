using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace EditorLogic
{
    public class MementoDrag
    {
        public readonly EditorObject Transformable;
        readonly Transform2 _transform;
        readonly IWall _parent;
        readonly IPolygonCoord _polygonCoord;

        public MementoDrag(EditorObject transformable)
        {
            Transformable = transformable;
            _polygonCoord = transformable.GetPolygonCoord();
            _transform = null;
            if (_polygonCoord != null)
            {
                _parent = (IWall)transformable.Parent;
            }
            else
            {
                _transform = transformable.GetTransform();
            }
        }

        public void ResetTransform()
        {
            if (_polygonCoord == null)
            {
                Transformable.SetTransform(_transform);
            }
            else
            {
                Transformable.SetTransform(_parent, _polygonCoord);
            }
        }

        public Transform2 GetTransform()
        {
            if (_transform == null)
            {
                return PolygonExt.GetTransform(_parent.Vertices, _polygonCoord);
            }
            return _transform.ShallowClone();
        }
    }
}

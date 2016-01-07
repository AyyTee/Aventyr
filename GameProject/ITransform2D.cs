using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ITransform2D
    {
        Transform2D GetTransform();
        void SetTransform(Transform2D transform);
        void SetPosition(Vector2 position);
        void SetRotation(float rotation);
        void SetScale(Vector2 scale);
    }
}

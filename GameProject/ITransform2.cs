using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ITransform2
    {
        Transform2 GetTransform();
        void SetTransform(Transform2 transform);
    }
}

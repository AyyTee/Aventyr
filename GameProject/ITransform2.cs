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
        /// <summary>Returns a copy of the local transform.</summary>
        Transform2 GetTransform();
        /// <summary>Replaces the local transform with a copy of the passed argument.</summary>
        void SetTransform(Transform2 transform);
    }
}

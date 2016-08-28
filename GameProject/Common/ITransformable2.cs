using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ITransformable2 : IGetTransform
    {
        /// <summary>Replaces the local transform with a copy of the passed argument.</summary>
        void SetTransform(Transform2 transform);
    }
}

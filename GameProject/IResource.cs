using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IResource<T>
    {
        /*ResourceMap<T>.ResourceID ID
        {
            get;
        }*/

        void SetID();
    }
}

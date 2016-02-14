using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IDeepClone
    {
        List<IDeepClone> GetCloneableRefs();
        IDeepClone ShallowClone();
        /// <summary>Used to update the references in the cloned instance.</summary>
        /// <param name="cloneMap">A dictionary containing instances as keys and their clones as values.</param>
        void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap);
    }
}

using System.Collections.Generic;

namespace Game.Serialization
{
    public interface IDeepClone : IShallowClone<IDeepClone>
    {
        HashSet<IDeepClone> GetCloneableRefs();
        /// <summary>Used to update the references in the cloned instance.</summary>
        /// <param name="cloneMap">A dictionary containing instances as keys and their clones as values.</param>
        void UpdateRefs(IReadOnlyDictionary<IDeepClone, IDeepClone> cloneMap);
    }
}

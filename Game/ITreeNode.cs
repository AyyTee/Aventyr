using System.Collections.Generic;

namespace Game
{
    public interface ITreeNode<T> where T : ITreeNode<T>
    {
        T Parent { get; }
        List<T> Children { get; }
    }
}

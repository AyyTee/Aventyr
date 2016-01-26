using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ITreeNode<T> where T : ITreeNode<T>
    {
        T Parent { get; }
        List<T> Children { get; }
    }
}

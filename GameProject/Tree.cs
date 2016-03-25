using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class Tree<T> where T : class, ITreeNode<T>
    {
        /// <summary>Returns true if child is a descendent of parent sceneNode.</summary>
        public static bool IsDescendent(T child, T anscestor)
        {
            if (child.Parent == null)
            {
                return false;
            }
            if (child.Parent.Equals(anscestor))
            {
                return true;
            }
            return IsDescendent(child.Parent, anscestor);
        }

        /// <summary>
        /// Returns true if there is a loop that leads back to this node.
        /// </summary>
        public static bool ParentLoopExists(T node)
        {
            HashSet<T> map = new HashSet<T>();
            while (node.Parent != null)
            {
                node = node.Parent;
                if (!map.Add(node))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a tree's nodes have Parent and Children pointers matching.
        /// </summary>
        public static bool IsValid(T root)
        {
            foreach (T child in root.Children)
            {
                if (child.Parent != root)
                {
                    return false;
                }
                if (!IsValid(child))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<T> ToList(T root)
        {
            return FindByType<T>(root);
        }

        public static List<S> FindByType<S>(T root) where S : class, T
        {
            List<S> list = new List<S>();
            S nodeCast = root as S;
            if (nodeCast != null)
            {
                list.Add(nodeCast);
            }
            foreach (T p in root.Children)
            {
                list.AddRange(Tree<T>.FindByType<S>(p));
            }
            return list;
        }

        /// <summary>
        /// Check if two trees are isomorphic (same shape and nodes equal eachother).
        /// </summary>
        public static bool Isomorphic(T Root0, T Root1)
        {
            throw new NotImplementedException();
        }
    }
}

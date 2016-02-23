using Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class EditorClone
    {
        public static void DeepClone(EditorScene source, EditorScene destination)
        {
            HashSet<IDeepClone> cloneList = new HashSet<IDeepClone>();
            GetReferences(new List<IDeepClone>(source.Children), cloneList);
            DeepClone(cloneList, destination);
        }

        public static void DeepClone(List<IDeepClone> source, EditorScene destination)
        {
            HashSet<IDeepClone> cloneList = new HashSet<IDeepClone>();
            GetReferences(new List<IDeepClone>(source), cloneList);
            DeepClone(cloneList, destination);
        }

        public static void DeepClone(IDeepClone source, EditorScene destination)
        {
            if (source == null)
            {
                return;
            }
            HashSet<IDeepClone> cloneList = new HashSet<IDeepClone>();
            GetReferences(source, cloneList);
            DeepClone(cloneList, destination);
        }

        private static void DeepClone(HashSet<IDeepClone> cloneHash, EditorScene destination)
        {
            Dictionary<IDeepClone, IDeepClone> cloneMap = new Dictionary<IDeepClone, IDeepClone>();
            foreach (IDeepClone original in cloneHash)
            {
                IDeepClone clone = original.ShallowClone();
                Debug.Assert(clone.GetType() == original.GetType(), "Type of cloned instance must match type of original instance.");
                cloneMap.Add(original, clone);
            }
            Debug.Assert(cloneMap.Count == cloneHash.Count);
            ReadOnlyDictionary<IDeepClone, IDeepClone> readOnlyCloneMap = new ReadOnlyDictionary<IDeepClone, IDeepClone>(cloneMap);
            foreach (IDeepClone clone in readOnlyCloneMap.Values)
            {
                clone.UpdateRefs(readOnlyCloneMap);
            }
            foreach (IDeepClone clone in readOnlyCloneMap.Values)
            {
                if (clone is EditorObject)
                {
                    ((EditorObject)clone).SetScene(destination);
                }
                else if (clone is SceneNode)
                {
                    ((SceneNode)clone).SetScene(destination.Scene);
                }
            }
        }

        public static void GetReferences(List<IDeepClone> entities, HashSet<IDeepClone> cloneList)
        {
            foreach (IDeepClone e in entities)
            {
                GetReferences(e, cloneList);
            }
        }

        public static void GetReferences(IDeepClone entity, HashSet<IDeepClone> cloneList)
        {
            cloneList.Add(entity);
            foreach (IDeepClone cloneable in entity.GetCloneableRefs())
            {
                GetReferences(cloneable, cloneList);
            }
        }
    }
}

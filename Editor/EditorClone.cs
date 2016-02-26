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
        public static void Clone(EditorScene source, EditorScene destination)
        {
            Clone(new List<IDeepClone>(source._children), destination);
        }

        public static void Clone(List<IDeepClone> source, EditorScene destination)
        {
            Debug.Assert(source != null);
            HashSet<IDeepClone> cloned = DeepClone.Clone(source);
            SetScene(cloned, destination);
        }

        public static void Clone(IDeepClone source, EditorScene destination)
        {
            Debug.Assert(source != null);
            List<IDeepClone> sourceList = new List<IDeepClone>();
            sourceList.Add(source);
            Clone(sourceList, destination);
        }

        private static void SetScene(HashSet<IDeepClone> cloned, EditorScene destination)
        {
            foreach (IDeepClone clone in cloned)
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
    }
}

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
        public void DeepClone(IDeepClone entity, EditorScene scene)
        {
            List<IDeepClone> cloneList = new List<IDeepClone>();
            GetReferences(entity, cloneList);
            Dictionary<IDeepClone, IDeepClone> cloneMap = new Dictionary<IDeepClone, IDeepClone>();
            foreach (IDeepClone cloneable in cloneList)
            {
                IDeepClone clone = cloneable.ShallowClone();
                Debug.Assert(clone.GetType() == cloneable.GetType(), "Type of cloned instance must match type of original instance.");
                cloneMap.Add(cloneable, clone);

                if (clone is EditorObject)
                {
                    ((EditorObject)clone).SetScene(scene);
                }
                else if (clone is SceneNode)
                {
                    ((SceneNode)clone).SetScene(scene.Scene);
                }
            }
            ReadOnlyDictionary<IDeepClone, IDeepClone> readOnlyCloneMap = new ReadOnlyDictionary<IDeepClone, IDeepClone>(cloneMap);
            foreach (IDeepClone clone in cloneList)
            {
                clone.UpdateRefs(readOnlyCloneMap);
            }
        }

        public void GetReferences(IDeepClone entity, List<IDeepClone> cloneList)
        {
            cloneList.Add(entity);
            foreach (IDeepClone cloneable in entity.GetCloneableRefs())
            {
                GetReferences(entity, cloneList);
            }
        }
    }
}

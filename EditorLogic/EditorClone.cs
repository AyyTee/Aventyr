using Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Serialization;
using Game.Common;

namespace EditorLogic
{
    public class EditorClone
    {
        public static List<EditorObject> Clone(EditorScene source, EditorScene destination)
        {
            return Clone(new List<IDeepClone>(source.Children), destination);
        }

        public static List<EditorObject> Clone(List<IDeepClone> source, EditorScene destination)
        {
            DebugEx.Assert(source != null);
            List<IDeepClone> cloned = DeepClone.Clone(source).Values.ToList();
            SetScene(cloned, destination);
            return cloned.OfType<EditorObject>().ToList();
        }

        public static List<EditorObject> Clone(IDeepClone source, EditorScene destination)
        {
            DebugEx.Assert(source != null);
            List<IDeepClone> sourceList = new List<IDeepClone>();
            sourceList.Add(source);
            return Clone(sourceList, destination);
        }

        public static void SetScene(IEnumerable<IDeepClone> cloned, EditorScene destination)
        {
            /*Contract.Requires(cloned != null);
            Contract.*/
            /*DebugEx.Assert(cloned != null);
            DebugEx.Assert(destination != null);*/
            foreach (IDeepClone clone in cloned)
            {
                if (clone is EditorObject)
                {
                    ((EditorObject)clone).SetScene(destination);
                }
                else if (clone is SceneNode)
                {
                    SceneNode sceneNode = (SceneNode)clone;
                    if (!cloned.Contains(sceneNode.Parent))
                    {
                        //sceneNode.SetParent(destination.Scene.Root);
                    }
                }
            }
        }
    }
}

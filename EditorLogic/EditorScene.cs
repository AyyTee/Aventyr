using FarseerPhysics.Dynamics;
using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace EditorLogic
{
    [DataContract]
    public class EditorScene : IRenderLayer, IScene
    {
        [DataMember]
        public List<EditorObject> _children = new List<EditorObject>();
        [DataMember]
        public ControllerCamera ActiveCamera { get; set; }
        [DataMember]
        public List<Doodad> Doodads = new List<Doodad>();
        [DataMember]
        public float Time { get; set; }

        public EditorScene()
        {
            #region create background
            Model background = Game.ModelFactory.CreatePlane();
            background.Texture = Renderer.GetTexture("grid.png");
            background.SetColor(new Vector3(1, 1, 0.5f));
            background.Transform.Position = new Vector3(0, 0, -5f);
            float size = 50;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Size = size;
            Doodad back = new Doodad();
            back.Models.Add(background);
            Doodads.Add(back);
            #endregion
        }

        public List<ISceneObject> GetAll()
        {
            HashSet<ISceneObject> set = new HashSet<ISceneObject>();
            foreach (EditorObject e in _children)
            {
                set.UnionWith(Tree<EditorObject>.GetDescendents(e));
            }
            set.Add(ActiveCamera);
            return set.ToList();
        }

        public void Clear()
        {
            List<EditorObject> childrenCopy = new List<EditorObject>(_children);
            foreach (EditorObject e in childrenCopy)
            {
                e.Remove();
            }
        }

        public List<IRenderable> GetRenderList()
        {
            List<IRenderable> renderList = GetAll().OfType<IRenderable>().ToList();
            renderList.AddRange(Doodads);
            return renderList;
        }

        public List<IPortal> GetPortalList()
        {
            return GetAll().OfType<IPortal>().ToList();
        }

        public ICamera2 GetCamera()
        {
            return ActiveCamera;
        }

        public void Step(float stepSize)
        {
            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepBegin(stepSize);
            }
            foreach (EditorObject s in GetAll().OfType<EditorObject>())
            {
                //Parented EditorObjects can't perform portal teleportation.
                if (s.Parent != null)
                {
                    SceneExt.RayCast(s, GetPortalList(), stepSize);
                }
                else
                {
                    s.SetTransform(s.GetTransform().Add(s.GetVelocity().Multiply(stepSize)));
                }
            }
            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepEnd(stepSize);
            }
        }
    }
}

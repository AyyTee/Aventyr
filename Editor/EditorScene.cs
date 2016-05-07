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

namespace Editor
{
    [DataContract]
    public class EditorScene : IRenderLayer, IScene
    {
        [DataMember]
        public World World;
        [DataMember]
        public List<EditorObject> _children = new List<EditorObject>();
        [DataMember]
        public ControllerCamera ActiveCamera { get; set; }
        public List<EditorObject> EditorObjects { get { return new List<EditorObject>(_children); } }
        public List<Doodad> Doodads = new List<Doodad>();

        public EditorScene()
        {
            World = new World(new Xna.Vector2(0, 9.8f));

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

        public List<EditorObject> FindAll()
        {
            return FindByType<EditorObject>();
        }

        public List<T> FindByType<T>() where T : EditorObject
        {
            List<T> list = new List<T>();
            foreach (EditorObject e in _children)
            {
                list.AddRange(Tree<EditorObject>.FindByType<T>(e));
            }
            return list;
        }

        public void Clear()
        {
            foreach (EditorObject e in EditorObjects)
            {
                e.Remove();
            }
        }

        public List<IRenderable> GetRenderList()
        {
            List<IRenderable> renderList = FindAll().OfType<IRenderable>().ToList();
            renderList.AddRange(Doodads);
            return renderList;
        }

        public List<IPortal> GetPortalList()
        {
            return FindAll().OfType<IPortal>().ToList();
        }

        public ICamera2 GetCamera()
        {
            return ActiveCamera;
        }

        public void Step(float stepSize)
        {
            foreach (EditorObject s in FindAll())
            {
                //Parented SceneNodes can't perform portal teleportation directly.
                if (s.Parent != null)
                {
                    SceneExt.RayCast(s, GetPortalList());
                }
                else
                {
                    s.SetTransform(s.GetTransform().Add(s.GetVelocity()));
                }
            }
            SceneExt.RayCast(ActiveCamera, GetPortalList());
        }
    }
}

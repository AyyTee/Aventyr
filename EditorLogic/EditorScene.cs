using FarseerPhysics.Dynamics;
using Game;
using Game.Animation;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using Game.Physics;
using Game.Rendering;
using Xna = Microsoft.Xna.Framework;

namespace EditorLogic
{
    [DataContract]
    public class EditorScene : IRenderLayer, IScene
    {
        public Renderer Renderer { get; private set; }
        [DataMember]
        public List<EditorObject> Children = new List<EditorObject>();
        [DataMember]
        public ControllerCamera ActiveCamera { get; set; }
        [DataMember]
        public List<Doodad> Doodads = new List<Doodad>();
        [DataMember]
        public double Time { get; private set; }

        public EditorScene(Renderer renderer = null)
        {
            Renderer = renderer;

            #region create background
            Model background = Game.Rendering.ModelFactory.CreatePlane();
            background.Texture = Renderer?.GetTexture("grid.png");
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
            foreach (EditorObject e in Children)
            {
                set.UnionWith(Tree<EditorObject>.GetDescendents(e));
            }
            set.Add(ActiveCamera);
            return set.ToList();
        }

        public void Clear()
        {
            List<EditorObject> childrenCopy = new List<EditorObject>(Children);
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

        public List<IPortalable> GetPortalableList()
        {
            return GetAll().OfType<IPortalable>().ToList();
        }

        public void AddKeyframe(EditorObject instance, Transform2 keyframe)
        {
            AddKeyframe(instance, keyframe, (float)Time);
        }

        public void AddKeyframe(EditorObject instance, Transform2 keyframe, float time)
        {
            if (instance.AnimatedTransform == null)
            {
                instance.AnimatedTransform = new CurveTransform2();
            }
            instance.AnimatedTransform.AddKeyframe(time, keyframe);
        }

        public void SetTime(double time)
        {
            Time = time;
            foreach (EditorObject e in GetAll().OfType<EditorObject>())
            {
                if (e.AnimatedTransform != null)
                {
                    e.SetTransform(e.AnimatedTransform.GetTransform((float)Time));
                }
            }
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
                s.StepBegin(this, stepSize);
            }
            /*foreach (EditorObject s in GetAll().OfType<EditorObject>())
            {
                //Parented EditorObjects can't perform portal teleportation.
                if (s.Parent != null)
                {
                    Ray.Settings settings = new Ray.Settings();
                    settings.TimeScale = stepSize;
                    Ray.RayCast(s, GetPortalList().Where(item => item != s), settings);
                }
                else
                {
                    s.SetTransform(s.GetTransform().Add(s.GetVelocity().Multiply(stepSize)));
                }
            }*/
            PortalCommon.UpdateWorldTransform(this, true);
            SimulationStep.Step(GetAll().OfType<IPortalCommon>(), GetAll().OfType<IPortal>(), stepSize, null);
            foreach (IStep s in GetAll().OfType<IStep>())
            {
                s.StepEnd(this, stepSize);
            }
        }

        public void SetRenderer(Renderer renderer)
        {
            Renderer = renderer;
        }
    }
}

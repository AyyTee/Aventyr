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
using OpenTK.Graphics;

namespace EditorLogic
{
    [DataContract]
    public class EditorScene : IScene
    {
        [DataMember]
        public List<EditorObject> Children = new List<EditorObject>();
        [DataMember]
        public ControllerCamera ActiveCamera { get; set; }
        [DataMember]
        public List<Doodad> Doodads = new List<Doodad>();
        [DataMember]
        public double Time { get; private set; }
        public bool RenderPortalViews => true;
        public IVirtualWindow Window { get; private set; }

        public EditorScene(IVirtualWindow window)
        {
            Window = window;

            #region create background
            float size = 50;
            Model background = Game.Rendering.ModelFactory.CreatePlane(Vector2.One * size, new Vector3(-size/2, -size/2, 0));
            background.Texture = Window.Textures?.Grid;
            background.SetColor(new Color4(1f, 1f, 0.5f, 1f));
            background.Transform.Position = new Vector3(0, 0, -5f);
            background.TransformUv.Size = size;
            Doodad back = new Doodad("Background");
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
            set.UnionWith(Doodads);
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

        public List<IPortalable> GetPortalableList() => GetAll().OfType<IPortalable>().ToList();


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

        public List<IPortal> GetPortalList() => GetAll().OfType<IPortal>().ToList();

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
    }
}

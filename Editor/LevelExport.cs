using FarseerPhysics.Dynamics;
using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public static class LevelExport
    {
        /// <summary>
        /// Creates a Scene from an EditorScene.  Scene is intended for gameplay use.
        /// </summary>
        public static Scene Export(EditorScene level)
        {
            Scene scene = new Scene();

            #region create background
            Model background = Game.ModelFactory.CreatePlane();
            background.Texture = Renderer.Textures["grid.png"];
            background.SetColor(new Vector3(1, 1, 0.5f));
            background.Transform.Position = new Vector3(0, 0, -5f);
            float size = 50;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Size = size;
            Entity back = new Entity(scene, new Vector2(0f, 0f));
            back.Name = "Background";
            back.AddModel(background);
            back.IsPortalable = false;
            #endregion

            HashSet<IDeepClone> toClone = new HashSet<IDeepClone>();
            //ICamera2 camera = level.ActiveCamera;
            //toClone.Add(level.ActiveCamera);
            Dictionary<EditorObject, SceneNode> dictionary = new Dictionary<EditorObject, SceneNode>();

            List<EditorObject> editorObjects = level.FindAll();
            foreach (EditorObject e in editorObjects)
            {
                if (e is EditorPortal)
                {
                    EditorPortal cast = (EditorPortal)e;
                    if (cast.OnEdge)
                    {
                        FixturePortal portal = new FixturePortal(scene);
                        
                        dictionary.Add(cast, portal);
                    }
                    else
                    {
                        FloatPortal portal = new FloatPortal(scene);
                        portal.SetTransform(cast.GetTransform());
                        dictionary.Add(cast, portal);
                    }
                }
                else if (e is EditorEntity)
                {
                    EditorEntity cast = (EditorEntity)e;
                    Entity clone = new Entity(scene);
                    clone.AddModelRange(cast.Models);
                    clone.SetTransform(cast.GetTransform());
                    dictionary.Add(cast, clone);
                }
                /*else if (e is EditorWall)
                {
                    EditorWall cast = (EditorWall)e;
                    Body body = ActorFactory.CreatePolygon(scene.World, cast.GetTransform(), cast.Vertices);
                    dictionary.Add(cast, new Actor(scene, body));
                }*/
                else if (e is IWall)
                {
                    EditorObject cast = (EditorObject)e;

                    Transform2 t = cast.GetTransform();
                    //Body body = ActorFactory.CreatePolygon(scene.World, t, ((IWall)e).Vertices);
                    Actor actor = new Actor(scene, ((IWall)e).Vertices, t);
                    
                    Transform2 tEntity = new Transform2();
                    //tEntity.SetScale(t.Scale);
                    Entity entity = new Entity(scene, tEntity);
                    entity.SetParent(actor);
                    if (e is EditorWall)
                    {
                        EditorWall castWall = (EditorWall)e;
                        actor.Vertices = castWall.Vertices;
                        entity.AddModel(Game.ModelFactory.CreatePolygon(castWall.Vertices));
                        dictionary.Add(castWall, actor);
                    }
                    else if (e is EditorActor)
                    {
                        actor.Body.IsStatic = false;
                        EditorActor castActor = (EditorActor)e;
                        actor.Vertices = castActor.Vertices;
                        entity.AddModel(castActor.GetActorModel());
                        dictionary.Add(castActor, actor);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            foreach (EditorObject e in editorObjects)
            {
                SceneNode parent = e.Parent == null ? scene.Root : dictionary[e.Parent];
                SceneNode clone = dictionary[e];
                clone.SetParent(parent);
                if (clone is IPortal)
                {
                    if (clone is FixturePortal)
                    {
                        FixturePortal cast = (FixturePortal)clone;

                        cast.SetPosition((IWall)parent, ((EditorPortal)e).PolygonTransform);
                        Debug.Assert(((IWall)parent).Vertices.Count > 0);

                        IPortal portalEditor = (IPortal)e;
                        if (portalEditor.Linked != null)
                        {
                            cast.Linked = (IPortal)dictionary[(EditorPortal)portalEditor.Linked];
                        }
                    }
                    else if (clone is FloatPortal)
                    {
                        FloatPortal cast = (FloatPortal)clone;

                        IPortal portalEditor = (IPortal)e;
                        if (portalEditor.Linked != null)
                        {
                            cast.Linked = (IPortal)dictionary[(EditorPortal)portalEditor.Linked];
                        }
                    }
                }
                
            }

            //Dictionary<IDeepClone, IDeepClone> dictionary = DeepClone.Clone(toClone);
            /*Cast all the cloned instances to SceneNode.  
            There should not be any types other than SceneNode or its derived types.*/
            List<SceneNode> cloned = dictionary.Values.Cast<SceneNode>().ToList();

            SceneNode.SetScene(cloned, scene);
            scene.SetActiveCamera(level.ActiveCamera);
            return scene;
        }
    }
}

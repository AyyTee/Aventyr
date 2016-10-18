using FarseerPhysics.Dynamics;
using Game;
using Game.Animation;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic
{
    public static class LevelExport
    {
        /// <summary>
        /// Creates a Scene from an EditorScene.  Scene is intended for gameplay use.
        /// </summary>
        public static Scene Export(EditorScene level, InputExt input)
        {
            Scene scene = new Scene();
            /*if (level.GetAll().OfType<EditorPlayer>().Count() > 0)
            {
                Camera2 camera = new Camera2(scene);
                camera.SetTransform(new Transform2(new Vector2(), 10, 0));
                scene.SetActiveCamera(camera);
                if (level.ActiveCamera != null)
                {
                    camera.Aspect = level.ActiveCamera.Aspect;
                }
            }
            else*/
            {
                if (level.ActiveCamera != null)
                {
                    ControllerCamera camera = level.ActiveCamera.ShallowClone();
                    camera.Scene = scene;
                    scene.SetActiveCamera(camera);
                }
            }

            #region create background
            Model background = Game.ModelFactory.CreatePlane();
            background.Texture = Renderer.GetTexture("grid.png");
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


            Dictionary<EditorObject, SceneNode> dictionary = new Dictionary<EditorObject, SceneNode>();
            AnimationDriver animation = new AnimationDriver();
            scene.SceneObjectList.Add(animation);

            List<EditorObject> editorObjects = level.GetAll().OfType<EditorObject>().ToList();
            foreach (EditorObject e in editorObjects)
            {
                if (e is EditorPortal)
                {
                    EditorPortal cast = (EditorPortal)e;

                    Entity entity = new Entity(scene);
                    entity.IsPortalable = false;
                    entity.AddModel(ModelFactory.CreatePortal());
                    entity.ModelList[0].Transform.Position += new Vector3(0, 0, -2);

                    if (cast.OnEdge)
                    {
                        FixturePortal portal = new FixturePortal(scene);
                        portal.Name = cast.Name;
                        dictionary.Add(cast, portal);

                        entity.SetParent(portal);
                    }
                    else
                    {
                        FloatPortal portal = new FloatPortal(scene);
                        portal.Name = cast.Name;
                        portal.SetTransform(cast.GetTransform());
                        dictionary.Add(cast, portal);

                        entity.SetParent(portal);

                        if (cast.AnimatedTransform != null)
                        {
                            animation.Add(portal, cast.AnimatedTransform);
                            portal.SetTransform(cast.AnimatedTransform.GetTransform(0));
                        }
                        else
                        {
                            portal.SetTransform(cast.GetTransform());
                        }
                    }
                }
                else if (e is EditorEntity)
                {
                    EditorEntity cast = (EditorEntity)e;
                    Entity clone = new Entity(scene);
                    clone.Name = cast.Name;
                    clone.AddModelRange(cast.Models);

                    dictionary.Add(cast, clone);

                    if (cast.AnimatedTransform != null)
                    {
                        animation.Add(clone, cast.AnimatedTransform);
                        clone.SetTransform(cast.AnimatedTransform.GetTransform(0));
                    }
                    else
                    {
                        clone.SetTransform(cast.GetTransform());
                    }
                }
                else if (e is IWall)
                {
                    EditorObject cast = e;

                    Transform2 t = cast.GetTransform();
                    Actor actor = new Actor(scene, ((IWall)e).Vertices, t);
                    actor.Name = cast.Name;
                    Transform2 tEntity = new Transform2();
                    Entity entity = new Entity(scene, tEntity);
                    entity.Name = cast.Name;
                    entity.SetParent(actor);
                    if (e is EditorWall)
                    {
                        EditorWall castWall = (EditorWall)e;
                        actor.SetBodyType(BodyType.Static);
                        //actor.Vertices = castWall.Vertices;
                        entity.AddModel(Game.ModelFactory.CreatePolygon(castWall.Vertices));
                        //entity.AddModel(Game.ModelFactory.CreateActorDebug(actor));
                        dictionary.Add(castWall, actor);
                    }
                    else if (e is EditorActor)
                    {
                        //actor.SetVelocity(new Transform2(new Vector2(0.2f, 0)));
                        EditorActor castActor = (EditorActor)e;
                        //actor.Vertices = castActor.Vertices;
                        entity.AddModel(castActor.GetActorModel(castActor));
                        //entity.AddModel(Game.ModelFactory.CreateActorDebug(actor));
                        dictionary.Add(castActor, actor);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    if (cast.AnimatedTransform != null)
                    {
                        animation.Add(actor, cast.AnimatedTransform);
                        actor.SetTransform(cast.AnimatedTransform.GetTransform(0));
                        actor.SetBodyType(BodyType.Kinematic);
                    }
                    else
                    {
                        actor.SetTransform(cast.GetTransform());
                    }
                }
                else if (e is EditorPlayer)
                {
                    EditorPlayer cast = (EditorPlayer)e;
                    Player player = new Player(input);
                    Vector2[] polygon = PolygonFactory.CreateNGon(6, 0.5f, new Vector2());
                    Actor actor = new Actor(scene, polygon);
                    player.SetActor(actor);
                    actor.SetTransform(new Transform2(cast.GetWorldTransform().Position));

                    //player.Camera = (Camera2)scene.ActiveCamera;

                    Entity entity = new Entity(scene, new Transform2());
                    entity.Name = cast.Name;
                    entity.SetParent(actor);
                    entity.AddModel(Game.ModelFactory.CreatePolygon(polygon));

                    scene.SceneObjectList.Add(player);
                    dictionary.Add(cast, player.Actor);
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            foreach (EditorObject e in editorObjects)
            {
                SceneNode parent = e.Parent == null ? null : dictionary[e.Parent];
                SceneNode clone = dictionary[e];
                clone.SetParent(parent);
                if (clone is IPortal)
                {
                    if (clone is FixturePortal)
                    {
                        FixturePortal cast = (FixturePortal)clone;

                        cast.SetPosition((IWall)parent, ((EditorPortal)e).PolygonTransform, e.GetTransform().Size, e.GetTransform().MirrorX);
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

            
            PortalCommon.UpdateWorldTransform(scene);

            return scene;
        }
    }
}

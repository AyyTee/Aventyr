using Common;
using Game;
using Game.Common;
using Game.Models;
using Game.Portals;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class SceneRender
    {
        readonly IVirtualWindow _window;
        public Scene Scene { get; set; }
        readonly Model _grid;
        readonly ModelGroup _box;
        float _zoomFactor = 1;
        public GridCamera Camera;

        public SceneRender(IVirtualWindow window, Scene scene)
        {
            _window = window;

            var size = 100;
            _grid = ModelFactory.CreatePlane(new Vector2(size, size), Color4.DarkGray, new Vector3(-size/2, -size/2, 0));
            _grid.TransformUv = new Transform2(size: size);
            
            Scene = scene;
            Camera = new GridCamera(new Transform2(), (float)_window.CanvasSize.XRatio);

            _box = ModelResources.Box(_window.Resources).Load(_window);
        }

        public void Update(IVirtualWindow window)
        {
            if (window.HasFocus)
            {
                _zoomFactor *= (float)Math.Pow(1.2, -window.MouseWheelDelta());
            }
        }

        public Layer Render(double animationT)
        {
            float t = (float)animationT;

            var worldLayer = new Layer();
            var baseZoom = 15;

            var cameraTransform = new Transform2().WithSize(baseZoom * _zoomFactor);
            var cameraVelocity = new Vector2();

            var playerInstant = Scene.GetSceneInstant(Scene.CurrentTime).Entities.GetOrDefault(Scene.CurrentPlayer);
            var time = Scene.CurrentTime;
            if (playerInstant != null)
            {
                var (transform, timeOffset) = GetCameraTransformAndTime(
                    Scene.GetSceneInstant(Scene.CurrentTime),
                    Scene.CurrentPlayer, t, Scene.Portals.ToList());

                cameraTransform = transform
                    .WithSize(baseZoom * _zoomFactor);
                time += timeOffset;

                cameraVelocity = t == 0 || t == 1 ?
                    new Vector2() :
                    (Vector2)Scene.GetSceneInstant(Scene.CurrentTime).Entities[Scene.CurrentPlayer].PreviousVelocity;
            }

            var worldCamera = new GridCamera(cameraTransform, (float)_window.CanvasSize.XRatio);
            worldCamera.WorldVelocity = worldCamera.WorldVelocity.WithPosition(cameraVelocity);
            worldLayer.Camera = worldCamera;
            Camera = worldCamera;

            var portalView = PortalView.CalculatePortalViews(0, Scene.Portals, worldCamera, 30);

            // We need to render portals first so that we can place entities clipping the portals correctly.
            RenderPortalView(portalView, worldLayer, time, t, 0, true);
            RenderPortalView(portalView, worldLayer, time, t, 0, false);

            return worldLayer;
        }

        /// <summary>
        /// </summary>
        /// <param name="portalView"></param>
        /// <param name="worldLayer"></param>
        /// <param name="time"></param>
        /// <param name="t"></param>
        /// <param name="offsetCount"></param>
        /// <param name="renderPortals">If true, portals are rendered. If false, everything else is rendered.</param>
        /// <returns></returns>
        int RenderPortalView(PortalView portalView, Layer worldLayer, int time, float t, int offsetCount, bool renderPortals)
        {
            DebugEx.Assert((offsetCount == 0) == (portalView.PortalEntrance == null));

            var offset = GetOffset(offsetCount);
            if (!renderPortals)
            {
                var renderables = RenderInstant(Scene.GetSceneInstant(time), t, worldLayer.Portals, _window.Resources?.LatoRegular());
                renderables.Add(new Renderable { Models = new List<Model> { _grid }, IsPortalable = false });
                foreach (var renderable in renderables)
                {
                    if (portalView.PortalEntrance != null)
                    {
                        renderable.WorldTransform = renderable.WorldTransform.AddPosition(offset);
                    }
                }
                worldLayer.Renderables.AddRange(renderables);
            }

            var offsetCountNext = offsetCount;

            foreach (var view in portalView.Children)
            {
                var timeNext = time + ((TimePortal)view.PortalEntrance).TimeOffset;

                offsetCountNext++;
                if (renderPortals)
                {
                    AddViewPortals(view, worldLayer, offset, GetOffset(offsetCountNext));
                }
                offsetCountNext = RenderPortalView(view, worldLayer, timeNext, t, offsetCountNext, renderPortals);
            }
            return offsetCountNext;
        }

        void AddViewPortals(PortalView view, Layer worldLayer, Vector2 entranceOffset, Vector2 exitOffset)
        {
            var entrance = new SimplePortal(
                view.PortalEntrance.WorldTransform.AddPosition(entranceOffset));
            var exit = new SimplePortal(
                view.PortalEntrance.Linked.WorldTransform.AddPosition(exitOffset));

            entrance.OneSided = view.PortalEntrance.OneSided;
            exit.OneSided = view.PortalEntrance.Linked.OneSided;

            exit.Linked = entrance;
            entrance.Linked = exit;
            worldLayer.Portals.AddRange(new[] { entrance, exit });
        }

        Vector2 GetOffset(int offsetCount) => new Vector2((offsetCount % 2) * 100, (offsetCount / 2) * 100);

        public List<Renderable> RenderInstant(SceneInstant sceneInstant, float t, IEnumerable<IPortalRenderable> portals, Font font)
        {
            var output = new List<Renderable>();

            foreach (var gridEntity in sceneInstant.Entities.Keys.OfType<IGridEntity>())
            {
                var transform = GridEntityWorldPosition(sceneInstant, gridEntity, t, portals);
                //var transform = (Transform2)sceneInstant.Entities[gridEntity].Transform.ToTransform2d().AddPosition(new Vector2d(0.5));

                Renderable renderable = null;
                switch (gridEntity)
                {
                    case Player p:
                        {
                            var model = ModelFactory.CreateCircle(new Vector3(), 0.48f, 16, Color4.Black);
                            model.Transform.Position += new Vector3(0, 0, 2f);
                            renderable = new Renderable(transform);
                            renderable.Models.Add(model);
                            break;
                        }

                    case Block b:
                        {
                            var blockInstant = (BlockInstant)sceneInstant.Entities[b];

                            renderable = new Renderable(transform);
                            renderable.Models.AddRange(_box.Models);
                            break;
                        }
                }

                output.Add(renderable);
            }

            var floor = new Model(ModelFactory.CreatePlaneMesh(new Vector2(), Vector2.One))
            {
                Texture = _window.Resources.Floor()
            };
            output.AddRange(
                Scene.Floor.Select(
                    item => new Renderable(
                        new Transform2((Vector2)item),
                        new[] { floor }.ToList())
                    {
                        IsPortalable = false
                    }));

            var wallModels = GetWallModels(_window.Resources, Scene.Floor, ModelResources.Wall(_window.Resources).Load(_window));
            output.Add(new Renderable(new Transform2(), wallModels));

            foreach (var exit in Scene.Exits)
            {
                output.Add(CreateSquare(exit, 1, new Color4(0.8f, 1f, 1f, 0.3f)));
            }

            foreach (var portal in Scene.Portals)
            {
                if (portal.Linked != null)
                {
                    var seed =
                        (portal.Position.X + portal.Linked.Position.X) * 1000 +
                        portal.Position.Y + portal.Linked.Position.Y;
                    var random = new Random(seed);
                    var color = random.Choose(Color4.ForestGreen, Color4.Crimson, Color4.Chocolate);
                    var square = CreateSquare(portal.Position, 1, color);
                    square.Models[0].Transform.Position = new Vector3(0, 0, 0.001f);
                    output.Add(square);

                    var models = new List<Model>();
                    for (int i = 0; i < GridAngle.CardinalDirections; i++)
                    {
                        var angle = new GridAngle(i);
                        if (angle.Equals(portal.Direction))
                        {
                            continue;
                        }

                        if (font != null)
                        {
                            var text = font.GetModel(portal.TimeOffset.ToString(), Color4.White, 0.5f);
                            text.Transform.Scale = new Vector3(-0.014f, 0.014f, 0.014f);
                            var offset = (Vector2)angle.Vector *  0.1f;
                            text.Transform.Position = new Vector3(offset.X, offset.Y, 1);
                            text.Transform.Rotation = new Quaternion(new Vector3(0, 0, 1), (float)(angle.Radians - Math.PI / 2));
                            models.Add(text);
                        }
                    }

                    var renderable = new Renderable(new Transform2((Vector2)portal.Position + Vector2.One / 2), models);
                    output.Add(renderable);
                }
                var line = Draw.Line(new LineF(portal.GetWorldVerts()), Color4.Blue, 0.08f);
                line.Models[0].Transform.Position += new Vector3(0, 0, 0.5f);
                line.IsPortalable = false;
                output.Add(line);
            }

            return output;
        }

        static Transform2 GridEntityWorldPosition(SceneInstant sceneInstant, IGridEntity gridEntity, float t, IEnumerable<IPortalRenderable> portals)
        {
            var offset = Vector2d.One / 2;
            var velocity = (Vector2)sceneInstant[gridEntity].PreviousVelocity;
            var transform = sceneInstant[gridEntity].Transform.ToTransform2d();
            transform = transform.WithPosition(transform.Position + offset);
            var result = Ray.RayCast((Transform2)transform, new Transform2(-velocity * (1 - t)), portals, new Ray.Settings());

            return result.WorldTransform;
        }

        (Transform2 Transform, int TimeOffset) GetCameraTransformAndTime(SceneInstant sceneInstant, IGridEntity gridEntity, float t, IEnumerable<IPortalRenderable> portals)
        {
            var offset = Vector2d.One / 2;
            var velocity = (Vector2)sceneInstant[gridEntity].PreviousVelocity;
            var transform = sceneInstant[gridEntity].Transform.ToTransform2d();
            transform = transform.WithPosition(transform.Position + offset);
            var result = Ray.RayCast((Transform2)transform, new Transform2(-velocity * (1 - t)), portals, new Ray.Settings());

            // Something isn't working with the camera's rotation. This offset corrects for whatever that is.
            var rotationOffset = result.PortalsEntered.Count(item => !Portal.GetLinkedTransform(item.EnterData.EntrancePortal).MirrorX) * Math.PI;

            return ValueTuple.Create(
                result.WorldTransform.AddRotation((float)rotationOffset),
                result.PortalsEntered.Sum(item => ((TimePortal)item.EnterData.EntrancePortal).TimeOffset));
        }

        public static List<Model> GetWallModels(Resources resources, ISet<Vector2i> floor, ModelGroup wallModel)
        {
            var output = new List<Model>();

            var bounds = MathEx.Bounds(floor);

            bounds = new RectangleI(bounds.Position - new Vector2i(1, 1), bounds.Size + new Vector2i(2, 2));
            var wallEdges = new Dictionary<Vector2i, bool[]>();
            for (int y = 0; y < bounds.Size.Y + 1; y++)
            {
                for (int x = 0; x < bounds.Size.X + 1; x++)
                {
                    var pos = bounds.Position + new Vector2i(x, y);
                    if (floor.Contains(pos))
                    {
                        continue;
                    }

                    var directions = GridAngle.CardinalDirections * 2;
                    var wallEdge = new bool[directions];
                    for (int i = 0; i < directions; i++)
                    {
                        var angle = (Math.PI * 2 * i) / directions;
                        var vector = pos + (Vector2i)MathEx.AngleToVector(angle).Round();
                        if (floor.Contains(vector))
                        {
                            wallEdge[i] = true;
                            if (i % 2 == 0)
                            {
                                wallEdge[MathEx.ValueWrap(i - 1, directions)] = true;
                                wallEdge[MathEx.ValueWrap(i + 1, directions)] = true;
                            }
                        }
                    }

                    if (wallEdge.All(item => !item))
                    {
                        continue;
                    }

                    wallEdges.Add(pos, wallEdge);
                }
            }

            var mesh = new Mesh();
            var wallHeight = 2;

            var indices = new[]
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 5,
                0, 5, 6,
                0, 6, 7,
                0, 7, 8,
                0, 8, 1
            };
            foreach (var wallEdge in wallEdges)
            {
                var pos = wallEdge.Key;
                var highlight = wallEdge.Value;
                var uv0 = new Vector2();
                var uv1 = new Vector2(1, 1);
                var vertices = new[]
                {
                    new Vertex(new Vector3(pos.X + 0.5f, pos.Y + 0.5f, wallHeight), uv1),
                    new Vertex(new Vector3(pos.X + 1.0f, pos.Y + 0.5f, wallHeight), highlight[0] ? uv0 : uv1),
                    new Vertex(new Vector3(pos.X + 1.0f, pos.Y + 1.0f, wallHeight), highlight[1] ? uv0 : uv1),
                    new Vertex(new Vector3(pos.X + 0.5f, pos.Y + 1.0f, wallHeight), highlight[2] ? uv0 : uv1),
                    new Vertex(new Vector3(pos.X + 0.0f, pos.Y + 1.0f, wallHeight), highlight[3] ? uv0 : uv1),
                    new Vertex(new Vector3(pos.X + 0.0f, pos.Y + 0.5f, wallHeight), highlight[4] ? uv0 : uv1),
                    new Vertex(new Vector3(pos.X + 0.0f, pos.Y + 0.0f, wallHeight), highlight[5] ? uv0 : uv1),
                    new Vertex(new Vector3(pos.X + 0.5f, pos.Y + 0.0f, wallHeight), highlight[6] ? uv0 : uv1),
                    new Vertex(new Vector3(pos.X + 1.0f, pos.Y + 0.0f, wallHeight), highlight[7] ? uv0 : uv1)
                };

                var index = mesh.AddVertexRange(vertices);
                mesh.Indices.AddRange(indices.Select(item => item + index));
            }
            //output.Add(new Model(mesh) { Texture = resources.WallFade() });

            foreach (var floorTile in floor)
            {
                for (int i = 0; i < GridAngle.CardinalDirections; i++)
                {
                    var angle = new GridAngle(i);
                    if (!floor.Contains(floorTile + angle.Vector))
                    {
                        output.AddRange(
                            wallModel.Models.Select(item =>
                            {
                                var clone = item.ShallowClone();
                                clone.Transform = new Transform3(
                                    new Vector3(floorTile.X + 0.5f, floorTile.Y + 0.5f, 0),
                                    Vector3.One,
                                    new Quaternion(new Vector3(0, 0, 1), (float)angle.Radians));
                                return clone;
                            }));
                    }
                }
            }

            return output;
        }

        static Renderable CreateSquare(Vector2i position, int size, Color4 color)
        {
            var model = ModelFactory.CreatePlane(Vector2.One * size * 0.98f, color, new Vector3(-size / 2f + 0.01f, -size / 2f + 0.01f, 0));
            return new Renderable(new Transform2((Vector2)position + Vector2.One * 0.5f * size))
            {
                Models = new List<Model> { model }
            };
        }

        public Vector2 GetMouseGrid()
        {
            return Camera
                .ScreenToWorld(_window.MousePosition, _window.CanvasSize)
                .Floor(Vector2.One);
        }
    }
}

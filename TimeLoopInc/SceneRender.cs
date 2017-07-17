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
        readonly Scene _scene;
        readonly Model _grid;
        float _zoomFactor = 1;
        public GridCamera Camera;

        public SceneRender(IVirtualWindow window, Scene scene)
        {
            _grid = ModelFactory.CreateGrid(new Vector2i(20, 20), Vector2.One, Color4.HotPink, Color4.LightPink, new Vector3(-10, -10, -2));
            _window = window;
            _scene = scene;
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

            var cameraTransform = new Transform2().WithSize(25 * _zoomFactor);
            var cameraVelocity = new Vector2();

            var playerInstant = _scene.CurrentInstant.Entities.GetOrDefault(_scene.CurrentPlayer);
            var time = _scene.CurrentTime;
            if (playerInstant != null)
            {
                var (transform, timeOffset) = GetCameraTransformAndTime(_scene.CurrentInstant, _scene.CurrentPlayer, t, _scene.Portals.ToList());

                cameraTransform = transform
                    .WithSize(25 * _zoomFactor);
                time += timeOffset;

                cameraVelocity = t == 0 || t == 1 ?
                    new Vector2() :
                    (Vector2)_scene.CurrentInstant.Entities[_scene.CurrentPlayer].PreviousVelocity;
            }

            var worldCamera = new GridCamera(cameraTransform, (float)_window.CanvasSize.XRatio);
            worldCamera.WorldVelocity = worldCamera.WorldVelocity.WithPosition(cameraVelocity / 6f);
            worldLayer.Camera = worldCamera;
            Camera = worldCamera;

            var portalView = PortalView.CalculatePortalViews(0, _scene.Portals, worldCamera, 30);

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
                var renderables = RenderInstant(_scene.GetSceneInstant(time), t, worldLayer.Portals);
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
                // If there isn't any time offset then we can skip rendering a duplicate of the scene.
                if (time == timeNext)
                {
                    if (renderPortals)
                    {
                        var entranceIndex = portalView.Children.IndexOf(view);
                        var exitIndex = portalView.Children
                            .IndexOfFirstOrNull(item => item.PortalEntrance == view.PortalEntrance.Linked);
                        DebugEx.Assert(entranceIndex != exitIndex);
                        if (exitIndex != null && entranceIndex < exitIndex)
                        {
                            AddViewPortals(view, worldLayer, offset, offset);
                        }
                    }
                }
                else
                {
                    offsetCountNext++;
                    if (renderPortals)
                    {
                        AddViewPortals(view, worldLayer, offset, GetOffset(offsetCountNext));
                    }
                    offsetCountNext = RenderPortalView(view, worldLayer, timeNext, t, offsetCountNext, renderPortals);
                }
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

        List<Renderable> RenderInstant(SceneInstant sceneInstant, float t, IEnumerable<IPortalRenderable> portals)
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
                            var model = ModelFactory.CreatePlane(Vector2.One * 0.98f, new Color4(), new Vector3(-0.49f));
                            model.SetColor(Color4.Black);

                            renderable = new Renderable(transform);
                            renderable.Models.Add(model);
                            break;
                        }

                    case Block b:
                        {
                            var blockInstant = (BlockInstant)sceneInstant.Entities[b];
                            var model = ModelFactory.CreatePlane(Vector2.One * blockInstant.Transform.Size * 0.98f, new Color4(), new Vector3(-0.49f));
                            model.SetColor(new Color4(0.5f, 1f, 0.8f, 1f));

                            renderable = new Renderable(transform);
                            renderable.Models.Add(model);
                            break;
                        }
                }

                output.Add(renderable);
            }

            foreach (var wall in _scene.Walls)
            {
                output.Add(CreateSquare(wall, 1, new Color4(0.8f, 1f, 0.5f, 1f)));
            }

            foreach (var exit in _scene.Exits)
            {
                output.Add(CreateSquare(exit, 1, new Color4(0.8f, 1f, 1f, 0.3f)));
            }

            output.Add(new Renderable { Models = new List<Model> { _grid }, IsPortalable = false });
            return output;
        }

        Transform2 GridEntityWorldPosition(SceneInstant sceneInstant, IGridEntity gridEntity, float t, IEnumerable<IPortalRenderable> portals)
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

            return ValueTuple.Create(
                result.WorldTransform,
                result.PortalsEntered.Sum(item => ((TimePortal)item.EnterData.EntrancePortal).TimeOffset));
        }


        Renderable CreateSquare(Vector2i position, int size, Color4 color)
        {
            var model = ModelFactory.CreatePlane(Vector2.One * size * 0.98f, new Color4(), new Vector3(-size / 2f + 0.01f));
            model.SetColor(color);

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

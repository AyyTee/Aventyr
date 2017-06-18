using Game;
using Game.Common;
using Game.Models;
using Game.Portals;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class SceneRender
    {
        readonly Scene _scene;
        readonly Model _grid;
        float _zoomFactor = 1;

        public SceneRender(Scene scene)
        {
            _grid = ModelFactory.CreateGrid(new Vector2i(20, 20), Vector2.One, Color4.HotPink, Color4.LightPink, new Vector3(-10, -10, -2));
            _scene = scene;
        }

        public void Update(IVirtualWindow window)
        {
            _zoomFactor *= (float)Math.Pow(1.2, -window.MouseWheelDelta());
        }

        public void Render(IVirtualWindow window, int _updatesSinceLastStep, int _updatesPerAnimation)
        {
            float t = MathHelper.Clamp(_updatesSinceLastStep / (float)_updatesPerAnimation, 0, 1);

            window.Layers.Clear();
            var worldLayer = new Layer();

            var cameraTransform = new Transform2().WithSize(25 * _zoomFactor);
            var cameraVelocity = new Vector2();

            var playerInstant = _scene.CurrentInstant.Entities.GetOrDefault(_scene.CurrentPlayer);
            if (playerInstant != null)
            {
                cameraTransform = GridEntityWorldPosition(_scene.CurrentInstant, _scene.CurrentPlayer, t, _scene.Portals.Cast<IPortalRenderable>())
                    .WithSize(25 * _zoomFactor);

                cameraVelocity = t == 0 || t == 1 ?
                    new Vector2() :
                    (Vector2)_scene.CurrentInstant.Entities[_scene.CurrentPlayer].PreviousVelocity;
            }
           
            var worldCamera = new GridCamera(cameraTransform, (float)window.CanvasSize.XRatio);

            var portalView = PortalView.CalculatePortalViews(0, _scene.Portals, worldCamera, 30);
            RenderPortalView(portalView, worldLayer, _scene.CurrentInstant.Time, t, 0);

            if (playerInstant != null)
            {
                worldCamera.WorldTransform = GridEntityWorldPosition(_scene.CurrentInstant, _scene.CurrentPlayer, t, worldLayer.Portals)
                    .WithSize(25 * _zoomFactor);
            }
            

            worldCamera.WorldVelocity = worldCamera.WorldVelocity.WithPosition(cameraVelocity / 6f);
            worldLayer.Camera = worldCamera;

            window.Layers.Add(worldLayer);
        }

        int RenderPortalView(PortalView portalView, Layer worldLayer, int time, float t, int offsetCount)
        {
            Debug.Assert((offsetCount == 0) == (portalView.PortalEntrance == null));
            var renderables = RenderInstant(_scene.GetStateInstant(time), t);

            var offset = GetOffset(offsetCount);
            foreach (var renderable in renderables)
            {
                if (portalView.PortalEntrance != null)
                {
                    renderable.WorldTransform = renderable.WorldTransform.AddPosition(offset);
                }
            }
            worldLayer.Renderables.AddRange(renderables);

            var offsetCountNext = offsetCount;

            foreach (var view in portalView.Children)
            {
                var timeNext = time + ((TimePortal)view.PortalEntrance).TimeOffset;
                // If there isn't any time offset then we can skip rendering a duplicate of the scene.
                if (time == timeNext)
                {
                    var entranceIndex = portalView.Children.IndexOf(view);
                    var exitIndex = portalView.Children.IndexOfFirstOrNull(item => item.PortalEntrance == view.PortalEntrance.Linked);
                    Debug.Assert(entranceIndex != exitIndex);
                    if (exitIndex != null && entranceIndex < exitIndex)
                    {
                        AddViewPortals(view, worldLayer, offset, offset);
                    }
                }
                else
                {
                    offsetCountNext++;
                    AddViewPortals(view, worldLayer, offset, GetOffset(offsetCountNext));
                    offsetCountNext = RenderPortalView(view, worldLayer, timeNext, t, offsetCountNext);
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

        List<Renderable> RenderInstant(SceneInstant sceneInstant, float t)
        {
            var output = new List<Renderable>();

            foreach (var gridEntity in sceneInstant.Entities.Keys.OfType<IGridEntity>())
            {
                //var transform = GridEntityWorldPosition(sceneInstant, gridEntity, t);
                var transform = (Transform2)sceneInstant[gridEntity].Transform.ToTransform2d().AddPosition(new Vector2d(0.5));

                Renderable renderable = null;
                switch (gridEntity)
                {
                    case Player p:
                        {
                            var model = ModelFactory.CreatePlane(Vector2.One * 0.98f, new Vector3(-0.49f));
                            model.SetColor(Color4.Black);

                            renderable = new Renderable(transform);
                            renderable.Models.Add(model);
                            break;
                        }

                    case Block b:
                        {
                            var blockInstant = (BlockInstant)sceneInstant.Entities[b];
                            var model = ModelFactory.CreatePlane(Vector2.One * blockInstant.Transform.Size * 0.98f, new Vector3(-0.49f));
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

            output.Add(new Renderable() { Models = new List<Model> { _grid }, IsPortalable = false });
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

        Renderable CreateSquare(Vector2i position, int size, Color4 color)
        {
            var model = ModelFactory.CreatePlane(Vector2.One * size * 0.98f,  new Vector3(-size / 2f + 0.01f));
            model.SetColor(color);

            return new Renderable(new Transform2((Vector2)position + Vector2.One * 0.5f * size))
            {
                Models = new List<Model> { model }
            };
        }
    }
}

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
            var state = _scene.State;
            var worldLayer = new Layer();

            var cameraTransform = new Transform2().WithSize(25 * _zoomFactor);
            var cameraVelocity = new Vector2();
            if (state.CurrentInstant.Entities.GetOrDefault(state.CurrentPlayer) != null)
            {
                cameraTransform = GridEntityWorldPosition(state.CurrentInstant, state.CurrentPlayer, t).WithSize(25 * _zoomFactor);
                cameraVelocity = t == 0 || t == 1 ?
                    new Vector2() :
                    (Vector2)state.CurrentInstant.Entities[state.CurrentPlayer].PreviousVelocity;
            }
           
            var worldCamera = new GridCamera(cameraTransform, (float)window.CanvasSize.XRatio);

            var portalView = PortalView.CalculatePortalViews(0, _scene.Portals, worldCamera, 30);
            RenderPortalView(portalView, worldLayer, state.CurrentInstant.Time, t, 0);


            
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
                offsetCountNext++;

                var entrance = new SimplePortal(
                    view.PortalEntrance.WorldTransform.AddPosition(offset));
                var exit = new SimplePortal(
                    view.PortalEntrance.Linked.WorldTransform.AddPosition(GetOffset(offsetCountNext)));

                exit.Linked = entrance;
                entrance.Linked = exit;

                worldLayer.Portals.AddRange(new[] { entrance, exit });
                
                var timeNext = time + ((TimePortal)view.PortalEntrance).TimeOffset;
                offsetCountNext = RenderPortalView(view, worldLayer, timeNext, t, offsetCountNext);
            }
            return offsetCountNext;
        }

        Vector2 GetOffset(int offsetCount) => new Vector2((offsetCount % 2) * 100, (offsetCount / 2) * 100);

        List<Renderable> RenderInstant(SceneInstant sceneInstant, float t)
        {
            var output = new List<Renderable>();
            var state = _scene.State;

            foreach (var gridEntity in sceneInstant.Entities.Keys.OfType<IGridEntity>())
            {
                var transform = GridEntityWorldPosition(sceneInstant, gridEntity, t);

                Renderable renderable = null;
                switch (gridEntity)
                {
                    case Player p:
                        {
                            var model = ModelFactory.CreatePlane(Vector2.One, new Vector3(-0.5f));
                            model.SetColor(Color4.Black);

                            renderable = new Renderable(transform);
                            renderable.Models.Add(model);
                            break;
                        }

                    case Block b:
                        {
                            var model = ModelFactory.CreatePlane(Vector2.One * b.Size, new Vector3(-0.5f));
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

        Transform2 GridEntityWorldPosition(SceneInstant sceneInstant, IGridEntity gridEntity, float t)
        {
            var offset = Vector2d.One / 2;
            var velocity = (Vector2)sceneInstant[gridEntity].PreviousVelocity;
            var transform = sceneInstant[gridEntity].Transform.ToTransform2d();
            transform = transform.WithPosition(transform.Position + offset);
            var result = Ray.RayCast((Transform2)transform, new Transform2(-velocity * (1 - t)), _scene.Portals, new Ray.Settings());

            return result.WorldTransform;
        }

        Renderable CreateSquare(Vector2i position, int size, Color4 color)
        {
            var model = ModelFactory.CreatePlane(Vector2.One * size,  new Vector3(-size / 2f));
            model.SetColor(color);

            return new Renderable(new Transform2((Vector2)position + Vector2.One * 0.5f * size))
            {
                Models = new List<Model> { model }
            };
        }
    }
}

using Game;
using Game.Common;
using Game.Models;
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
        readonly Scene _scene;
        readonly Model _grid;

        public SceneRender(Scene scene)
        {
            _grid = ModelFactory.CreateGrid(new Vector2i(20, 20), Vector2.One, Color4.HotPink, Color4.LightPink, new Vector3(-10, -10, -2));
            _scene = scene;

            
        }

        public void Render(IVirtualWindow window, int _updatesSinceLastStep, int _updatesPerAnimation)
        {
            float t = MathHelper.Clamp(_updatesSinceLastStep / (float)_updatesPerAnimation, 0, 1);

            window.Layers.Clear();
            var state = _scene.State;
            var worldLayer = new Layer();
            RenderInstant(state.CurrentInstant, worldLayer, t);

            var cameraTransform = GridEntityWorldPosition(state.CurrentPlayer, t).SetSize(25);
            var worldCamera = new GridCamera(cameraTransform, (float)window.CanvasSize.XRatio);

            var cameraVelocity = t == 1 || t == 0 ?
                new Vector2() :
                (Vector2)state.CurrentInstant.Entities[state.CurrentPlayer].PreviousVelocity;
            worldCamera.WorldVelocity = worldCamera.WorldVelocity.SetPosition(cameraVelocity / 6f);
            worldLayer.Camera = worldCamera;

            window.Layers.Add(worldLayer);
        }

        void RenderInstant(SceneInstant sceneInstant, Layer layer, float t)
        {
            var state = _scene.State;

            foreach (var gridEntity in state.CurrentInstant.Entities.Keys.OfType<IGridEntity>())
            {
                var transform = GridEntityWorldPosition(gridEntity, t);

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

                layer.Renderables.Add(renderable);
            }
            foreach (var portal in _scene.Portals)
            {
                layer.Renderables.Add(new Square(portal.Position) { Color = new Color4(0.6f, 0.8f, 0.8f, 1f) });
                layer.Portals.Add(portal);
            }
            foreach (var wall in _scene.Walls)
            {
                layer.Renderables.Add(new Square(wall) { Color = new Color4(0.8f, 1f, 0.5f, 1f) });
            }

            layer.Renderables.Add(new Renderable() { Models = new List<Model> { _grid }, IsPortalable = false });
        }

        Transform2 GridEntityWorldPosition(IGridEntity gridEntity, float t)
        {
            var offset = Vector2d.One / 2;
            var velocity = (Vector2)_scene.State.CurrentInstant[gridEntity].PreviousVelocity;
            var transform = _scene.State.CurrentInstant[gridEntity].Transform.ToTransform2d();
            transform = transform.SetPosition(transform.Position + offset);
            var result = Ray.RayCast((Transform2)transform, new Transform2(-velocity * (1 - t)), _scene.Portals, new Ray.Settings());

            return result.WorldTransform;
        }
    }
}

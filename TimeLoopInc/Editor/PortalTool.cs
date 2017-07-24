using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace TimeLoopInc.Editor
{
    public class PortalTool
    {
        readonly IVirtualWindow _window;

        public PortalTool(IVirtualWindow window)
        {
            _window = window;
        }

        /// <summary>
        /// Returns a modified scene or null if no changes have been made.
        /// </summary>
        public SceneBuilder Update(SceneBuilder scene, ICamera2 camera)
        {
            if (_window.ButtonPress(MouseButton.Left))
            {
                var mousePosition = _window.MouseWorldPos(camera);
                var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);

                var sides = EditorController.PortalValidSides(mouseGridPos, scene.Walls);
                if (sides.Count > 0)
                {


                    var side = sides
                        .OrderBy(item => ((Vector2)item.Vector - mousePosition.Frac(Vector2.One) + Vector2.One / 2).Length)
                        .First();

                    var newPortal = new PortalBuilder(mouseGridPos, side);
                    var links = scene.Links;
                    if (_window.ButtonDown(KeyBoth.Shift) &&
                        scene.Links.Any() &&
                        scene.Links.Last().Portals.Length == 1)
                    {
                        var previousLink = links.Last();
                        var newLink = new PortalLink(new[] { previousLink.Portals[0], newPortal });
                        links = links.Remove(previousLink).Add(newLink);
                    }
                    else
                    {
                        links = links.Add(new PortalLink(new[] { newPortal }));
                    }

                    var collisions = EditorController.PortalCollisions(newPortal, scene.Links.SelectMany(item => item.Portals));
                    links = EditorController.GetPortals(portal => !collisions.Contains(portal), links);

                    return scene.With(links: links);
                }
            }
            return null;
        }

        public List<IRenderable> Render(SceneBuilder scene, ICamera2 camera)
        {
            var output = new List<IRenderable>();

            var _mousePosition = _window.MouseWorldPos(camera);

            var previousLink = scene.Links.LastOrDefault();
            var previousPortal = previousLink?.Portals.Length == 1 ?
                previousLink.Portals[0] :
                null;
            if (_window.ButtonDown(KeyBoth.Shift) && previousPortal != null)
            {
                var line = Draw.Line(
                    new LineF(
                        (Vector2)previousPortal.Position + Vector2.One / 2 + (Vector2)previousPortal.Direction.Vector / 2,
                        _mousePosition),
                    Color4.Black,
                    0.04f);
                line.IsPortalable = false;
                line.DrawOverPortals = true;
                line.Models.ForEach(item => item.Transform.Position += new Vector3(0, 0, 10));
                output.Add(line);
            }

            return output;
        }
    }
}

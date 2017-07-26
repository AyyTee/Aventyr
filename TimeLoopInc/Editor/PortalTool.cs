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
    public class PortalTool : ITool
    {
        readonly IVirtualWindow _window;
        PortalBuilder _lastPlaced;

        public PortalTool(IVirtualWindow window)
        {
            _window = window;
        }

        /// <summary>
        /// Returns a modified scene or null if no changes have been made.
        /// </summary>
        public SceneBuilder Update(SceneBuilder scene, ICamera2 camera)
        {
            var mousePosition = _window.MouseWorldPos(camera);
            var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
            if (_window.ButtonPress(MouseButton.Left))
            {
                var sides = EditorController.PortalValidSides(mouseGridPos, scene.Walls);
                if (sides.Count > 0)
                {
                    var side = sides
                        .OrderBy(item => ((Vector2)item.Vector - mousePosition.Frac(Vector2.One) + Vector2.One / 2).Length)
                        .First();

                    var newPortal = new PortalBuilder(mouseGridPos, side);

                    var links = scene.Links;

                    // Remove any portals the new portal is overlapping.
                    var collisions = EditorController.PortalCollisions(newPortal, scene.Links.SelectMany(item => item.Portals));
                    links = EditorController.GetPortals(portal => !collisions.Contains(portal), links);

                    links = links.Add(new PortalLink(new[] { newPortal }));
                    if (_window.ButtonDown(KeyBoth.Shift) && _lastPlaced != null)
                    {
                        DebugEx.Assert(LinkTool.GetLink(_lastPlaced, links).Portals.Length == 1);
                        links = LinkTool.LinkPortals(_lastPlaced, newPortal, links);
                        _lastPlaced = null;
                    }
                    else
                    {
                        _lastPlaced = newPortal;
                    }

                    return scene.With(links: links);
                }
            }
            else if (_window.ButtonPress(MouseButton.Right))
            {
                return EditorController.Remove(scene, mouseGridPos);
            }
            return null;
        }

        public List<IRenderable> Render(SceneBuilder scene, ICamera2 camera)
        {
            var output = new List<IRenderable>();

            var _mousePosition = _window.MouseWorldPos(camera);

            var previousLink = scene.Links.LastOrDefault();
            if (_window.ButtonDown(KeyBoth.Shift) && _lastPlaced != null)
            {
                var line = Draw.Line(
                    new LineF(_lastPlaced.Center, _mousePosition),
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

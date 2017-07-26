using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Game.Common;
using Game.Rendering;
using MoreLinq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace TimeLoopInc.Editor
{
    public class LinkTool : ITool
    {
        readonly IEditorController _editor;
        PortalBuilder _selected;
        public float MaxSelectDistance { get; set; } = 100;

        public LinkTool(IEditorController editor)
        {
            _editor = editor;
        }

        public void Update()
        {
            var window = _editor.Window;
            var camera = _editor.Camera;
            var scene = _editor.Scene;
            var mousePosition = window.MouseWorldPos(camera);

            if (window.ButtonPress(MouseButton.Left))
            {
                if (_selected == null)
                {
                    var portals = scene.Links.SelectMany(item => item.Portals);
                    var nearest = NearestPortal(mousePosition, portals);
                    if (nearest != null)
                    {
                        var screenDistance = (window.MousePosition - camera.WorldToScreen(nearest.Center, window.CanvasSize)).Length;
                        if (screenDistance < MaxSelectDistance)
                        {
                            _selected = nearest;
                        }
                    }
                }
                else
                {
                    var portals = scene.Links
                    .SelectMany(item => item.Portals)
                    .Where(item => item != _selected);
                    var nearest = NearestPortal(mousePosition, portals);
                    if (nearest != null)
                    {
                        var screenDistance = (window.MousePosition - camera.WorldToScreen(nearest.Center, window.CanvasSize)).Length;
                        if (screenDistance < MaxSelectDistance)
                        {
                            var newLinks = LinkPortals(_selected, nearest, scene.Links);
                            _selected = null;
                            if (newLinks.SequenceEqual(scene.Links))
                            {
                                return;
                            }
                            _editor.ApplyChanges(scene.With(links: newLinks));
                        }
                    }
                }
            }
            else if (window.ButtonPress(MouseButton.Right))
            {
                if (_selected != null)
                {
                    _selected = null;
                }
                else
                {
                    var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
                    _editor.ApplyChanges(EditorController.Remove(scene, mouseGridPos));
                }
            }
        }

        public static PortalLink GetLink(PortalBuilder portal, IEnumerable<PortalLink> links)
        {
            if (portal == null)
            {
                return null;
            }
            return links.FirstOrDefault(item => item.Portals.Contains(portal));
        }

        public static ImmutableList<PortalLink> LinkPortals(PortalBuilder portal0, PortalBuilder portal1, IEnumerable<PortalLink> links)
        {
            DebugEx.Assert(portal0 != null);
            DebugEx.Assert(portal1 != null);
            var portals = new[] { portal0, portal1 };
            var previousLinks = new[]
            {
                links.FirstOrDefault(item => item.Portals.Contains(portal0)),
                links.FirstOrDefault(item => item.Portals.Contains(portal1))
            };
            if (previousLinks[0] == previousLinks[1])
            {
                return links.ToImmutableList();
            }
            if (previousLinks.All(item => item != null))
            {
                return links
                    .Select(item => new PortalLink(item.Portals.Where(portal => !portals.Contains(portal)).ToArray(), item.TimeOffset))
                    .Where(item => item.Portals.Length > 0)
                    .Concat(new PortalLink(new[] { portal0, portal1 }))
                    .ToImmutableList();
            }
            return links.ToImmutableList();
        }

        PortalBuilder NearestPortal(Vector2 position, IEnumerable<PortalBuilder> portals)
        {
            if (portals.Any())
            {
                return portals.MinBy(item => (item.Center - position).Length);
            }
            return null;
        }

        public List<IRenderable> Render()
        {
            var output = new List<IRenderable>();

            var _mousePosition = _editor.Window.MouseWorldPos(_editor.Camera);
            if (_selected != null)
            {
                var line = Draw.Line(new LineF(_selected.Center, _mousePosition), Color4.Black, 0.04f);
                output.Add(line);
            }
            return output;
        }
    }
}

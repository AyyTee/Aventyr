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

            var selectedPortal = SelectedPortal(scene);
            if (window.ButtonPress(_editor.PlaceButton))
            {
                if (selectedPortal == null)
                {
                    var portals = scene.Links.SelectMany(item => item.Portals);
                    var nearest = NearestPortal(mousePosition, portals);
                    if (nearest != null)
                    {
                        var screenDistance = (window.MousePosition - camera.WorldToScreen(nearest.Center, window.CanvasSize)).Length;
                        if (screenDistance < MaxSelectDistance)
                        {
                            selectedPortal = nearest;
                            _editor.ApplyChanges(scene.With(selectedPortal.Position), true);
                        }
                    }
                }
                else
                {
                    var portals = scene.Links
                        .SelectMany(item => item.Portals)
                        .Where(item => item != selectedPortal);
                    var nearest = NearestPortal(mousePosition, portals);
                    if (nearest != null)
                    {
                        var screenDistance = (window.MousePosition - camera.WorldToScreen(nearest.Center, window.CanvasSize)).Length;
                        if (screenDistance < MaxSelectDistance)
                        {
                            var newLinks = LinkPortals(selectedPortal, nearest, scene.Links);
                            if (newLinks.SequenceEqual(scene.Links))
                            {
                                return;
                            }
                            _editor.ApplyChanges(scene.With(links: newLinks).With(null));
                        }
                    }
                }
            }
            var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
            EditorController.DeleteAndSelect(_editor, mouseGridPos);
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

        public static PortalBuilder SelectedPortal(SceneBuilder scene)
        {
            return scene.Links
                .SelectMany(item => item.Portals)
                .FirstOrDefault(item => item.Position == scene.Selected);
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
            var selectedPortal = SelectedPortal(_editor.Scene);
            if (selectedPortal != null)
            {
                var line = Draw.Line(new LineF(selectedPortal.Center, _mousePosition), Color4.Black, 0.04f);
                output.Add(line);
            }
            return output;
        }
    }
}

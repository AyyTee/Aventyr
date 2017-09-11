using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Rendering;
using OpenTK.Input;
using OpenTK;
using Game.Common;
using OpenTK.Graphics;
using Game.Models;

namespace TimeLoopInc.Editor
{
    public class WallTool : ITool
    {
        readonly IEditorController _editor;

        public WallTool(IEditorController editor)
        {
            _editor = editor;
        }

        public List<IRenderable> Render()
        {
            var window = _editor.Window;
            var scene = _editor.Scene;
            if ((window.ButtonDown(MouseButton.Left) || window.ButtonDown(MouseButton.Right)) && scene.Selected != null)
            {
                var camera = _editor.Camera;
                
                var mousePosition = window.MouseWorldPos(camera);

                var color = window.ButtonDown(MouseButton.Left) ? 
                    new Color4(1, 1, 1, 0.5f) : 
                    new Color4(1, 0, 0, 0.5f);

                var selectionRegion = (RectangleF)GetSelection(mousePosition);

                var selection = new Model(
                    ModelFactory.CreatePlaneMesh(
                        selectionRegion.Position, 
                        selectionRegion.Position + selectionRegion.Size, 
                        selectionRegion.Size, 
                        color))
                {
                    Texture = _editor.Window.Resources.Floor()
                };

                return new List<IRenderable> { new Renderable(selection) };
            }
            return new List<IRenderable>();
        }

        RectangleI GetSelection(Vector2 mouseWorldPos)
        {
            var mouseGridPos = (Vector2i)mouseWorldPos.Floor(Vector2.One);
            var vMin = Vector2i.ComponentMin(_editor.Scene.Selected.Value, mouseGridPos);
            var vMax = Vector2i.ComponentMax(_editor.Scene.Selected.Value, mouseGridPos) + new Vector2i(1, 1);
            return new RectangleI(vMin, vMax - vMin);
        }

        public void Update()
        {
            var window = _editor.Window;
            var camera = _editor.Camera;
            var mousePosition = window.MouseWorldPos(camera);
            var mouseGridPos = (Vector2i)mousePosition.Floor(Vector2.One);
            var occupied = _editor.Scene.Floor.Contains(mouseGridPos);
            if (window.ButtonPress(MouseButton.Left))
            {
                if (!occupied)
                {
                    var walls = _editor.Scene.Floor.Add(mouseGridPos);
                    var links = EditorController.GetPortals(
                        portal => EditorController.PortalValidSides(portal.Position, walls).Any(),
                        _editor.Scene.Links);
                    _editor.ApplyChanges(_editor.Scene.With(walls, links: links));
                }
                _editor.ApplyChanges(_editor.Scene.With(mouseGridPos), true);
            }
            else if (window.ButtonPress(MouseButton.Right))
            {
                if (occupied)
                {
                    _editor.ApplyChanges(_editor.Scene.With(floor: _editor.Scene.Floor.Remove(mouseGridPos)));
                }
                _editor.ApplyChanges(_editor.Scene.With(mouseGridPos), true);
            }

            if (_editor.Scene.Selected != null)
            {
                var selection = GetSelection(mousePosition);
                if (window.ButtonRelease(MouseButton.Left))
                {
                    if (mouseGridPos != _editor.Scene.Selected)
                    {
                        var floor = Enumerable
                            .Range(0, selection.Size.X * selection.Size.Y)
                            .Select(item => selection.Position + new Vector2i(item % selection.Size.X, item / selection.Size.X));
                        _editor.ApplyChanges(_editor.Scene.With(floor: _editor.Scene.Floor.Union(floor)).With(mouseGridPos));
                    }
                }
                else if (window.ButtonRelease(MouseButton.Right))
                {
                    if (mouseGridPos != _editor.Scene.Selected)
                    {
                        var floor = _editor.Scene.Floor.Where(item => !MathEx.PointInRectangle(selection, item));
                        _editor.ApplyChanges(_editor.Scene.With(floor: new HashSet<Vector2i>(floor)).With(mouseGridPos));
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>Original code found here: http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html </remarks>
        public static IEnumerable<Vector2i> GetPointsOnLine(Vector2i v0, Vector2i v1)
        {
            var x0 = v0.X;
            var y0 = v0.Y;
            var x1 = v1.X;
            var y1 = v1.Y;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                yield return new Vector2i((steep ? y : x), (steep ? x : y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }
    }
}

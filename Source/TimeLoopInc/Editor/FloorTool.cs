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
using System.Collections.Immutable;

namespace TimeLoopInc.Editor
{
    public class FloorTool : ITool
    {
        readonly IEditorController _editor;

        public FloorTool(IEditorController editor)
        {
            _editor = editor;
        }

        public List<IRenderable> Render()
        {
            var window = _editor.Window;
            var scene = _editor.Scene;
            var output = new List<IRenderable>();
            if ((window.ButtonDown(MouseButton.Left) || window.ButtonDown(MouseButton.Right)) && scene.Selected != null)
            {
                var camera = _editor.Camera;
                
                var mousePosition = window.MouseWorldPos(camera);

                var fillColor = window.ButtonDown(MouseButton.Left) ? 
                    new Color4(0.5f, 0.5f, 1, 0.5f) : 
                    new Color4(1, 0, 0, 0.5f);
                var outlineColor = new Color4(fillColor.R, fillColor.G, fillColor.B, 1);

                var selectionRegion = (RectangleF)GetSelection(mousePosition);

                var selection = new Model(
                    ModelFactory.CreatePlaneMesh(
                        selectionRegion.Position, 
                        selectionRegion.Position + selectionRegion.Size, 
                        selectionRegion.Size, 
                        fillColor))
                {
                    Texture = _editor.Window.Resources.Floor(),
                };

                output.Add(new Renderable(selection));
                output.Add(
                    Draw.RectangleOutline(
                        selectionRegion.Position, 
                        selectionRegion.Position + selectionRegion.Size, 
                        fillColor, 
                        0.05f));
            }
            return output;
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
                _editor.ApplyChanges(_editor.Scene.With(mouseGridPos), true);
            }
            else if (window.ButtonPress(MouseButton.Right))
            {
                _editor.ApplyChanges(_editor.Scene.With(mouseGridPos), true);
            }

            if (_editor.Scene.Selected != null)
            {
                var selection = GetSelection(mousePosition);
                if (window.ButtonRelease(MouseButton.Left))
                {
                    var floor = _editor.Scene.Floor.Union(FloorRectangle(selection));
                    _editor.ApplyChanges(_editor.Scene.With(floor: floor).With(mouseGridPos));
                }
                else if (window.ButtonRelease(MouseButton.Right))
                {
                    var floor = _editor.Scene.Floor.Where(item => !MathEx.PointInRectangle(selection.With(size: selection.Size - new Vector2i(1, 1)), item));
                    _editor.ApplyChanges(_editor.Scene.With(floor: new HashSet<Vector2i>(floor)).With(mouseGridPos));
                }
            }
        }

        public static IEnumerable<Vector2i> FloorRectangle(RectangleI region)
        {
            return Enumerable
                .Range(0, region.Size.X * region.Size.Y)
                .Select(item => region.Position + new Vector2i(item % region.Size.X, item / region.Size.X));
        }
    }
}

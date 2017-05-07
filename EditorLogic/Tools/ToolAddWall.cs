using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Models;
using OpenTK;
using OpenTK.Input;
using Game.Rendering;
using OpenTK.Graphics;

namespace EditorLogic.Tools
{
    public class ToolAddWall : Tool
    {
        List<Vector2> _vertices = new List<Vector2>();
        Doodad _polygon;
        public ToolAddWall(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Enable()
        {
            base.Enable();
            _polygon = new Doodad(Controller.Level, "Add Wall Polygon");
            _polygon.IsPortalable = true;
        }

        public override void Disable()
        {
            _vertices.Clear();
            Controller.Level.Doodads.Remove(_polygon);
            base.Disable();
        }

        public override void Update()
        {
            base.Update();
            if (Input.ButtonPress(Key.Delete))
            {
                if (_vertices.Count > 0)
                {
                    _vertices.RemoveAt(_vertices.Count - 1);
                    UpdatePolygon();
                }
            }
            else if (Input.ButtonPress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (Input.ButtonPress(MouseButton.Left))
            {
                Vector2 mousePos = Controller.GetMouseWorld();
                if (mousePos != _vertices.LastOrDefault())
                {
                    if (_vertices.Count >= 3 && (mousePos - _vertices[0]).Length < 0.1f)
                    {
                        Vector2 average = new Vector2(_vertices.Average(item => item.X), _vertices.Average(item => item.Y));
                        for (int i = 0; i < _vertices.Count; i++)
                        {
                            _vertices[i] -= average;
                        }
                        _vertices = PolygonExt.SetNormals(_vertices);
                        _vertices = (List<Vector2>)MathExt.SetWinding(_vertices, false);
                        EditorWall editorEntity = new EditorWall(Controller.Level, _vertices);
                        editorEntity.SetPosition(average);
                        Controller.SetTool(null);
                    }
                    else
                    {
                        _vertices.Add(mousePos);
                        UpdatePolygon();
                    }
                }
            }
        }

        public void UpdatePolygon()
        {
            _polygon.Models.Clear();
            if (_vertices.Count() >= 2)
            {
                PolygonCoord[] intersects = MathExt.LineStripIntersect(_vertices.ToArray(), true);
                var colors = new Color4[_vertices.Count()];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Color4(0, 0.5f, 0, 1);
                }
                foreach (PolygonCoord p in intersects)
                {
                    colors[p.EdgeIndex] = new Color4(1f, 0.2f, 0.2f, 1f);
                }
                Model model = Game.Rendering.ModelFactory.CreateLineStrip(_vertices.ToArray(), colors);
                model.Transform.Position = new Vector3(0, 0, 6);
                //model.SetShader("default");
                _polygon.Models.Add(model);
            }
        }
    }
}

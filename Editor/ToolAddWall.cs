using Game;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
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
            _polygon = new Doodad(Controller.Level);
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
            if (_input.KeyPress(Key.Delete))
            {
                if (_vertices.Count > 0)
                {
                    _vertices.RemoveAt(_vertices.Count - 1);
                    UpdatePolygon();
                }
            }
            else if (_input.MousePress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (_input.MousePress(MouseButton.Left))
            {
                Vector2 mousePos = Controller.GetMouseWorldPosition();
                if (_vertices.Count >= 3 && (mousePos - _vertices[0]).Length < 0.1f)
                {
                    Vector2 average = new Vector2(_vertices.Average(item => item.X), _vertices.Average(item => item.Y));
                    for (int i = 0; i < _vertices.Count; i++)
                    {
                        _vertices[i] -= average;
                    }
                    PolygonExt.SetInterior(_vertices);
                    MathExt.SetHandedness(_vertices, false);
                    EditorWall editorEntity = new EditorWall(Controller.Level, _vertices);
                    Transform2.SetPosition(editorEntity, average);
                    Controller.SetTool(null);
                }
                else
                {
                    _vertices.Add(mousePos);
                    UpdatePolygon();
                }
            }
        }

        public void UpdatePolygon()
        {
            _polygon.Models.Clear();
            if (_vertices.Count() >= 2)
            {
                PolygonCoord[] intersects = MathExt.LineStripIntersect(_vertices.ToArray(), true);
                Vector3[] colors = new Vector3[_vertices.Count() - 1];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Vector3(0, 0.5f, 0);
                }
                foreach (PolygonCoord p in intersects)
                {
                    colors[p.EdgeIndex] = new Vector3(1f, 0.2f, 0.2f);
                }
                Model model = Game.ModelFactory.CreateLineStrip(_vertices.ToArray(), colors);
                model.Transform.Position = new Vector3(0, 0, 6);
                //model.SetShader("default");
                _polygon.Models.Add(model);
            }
        }
    }
}

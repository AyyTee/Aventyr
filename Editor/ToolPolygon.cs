using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using Game;

namespace Editor
{
    public class ToolPolygon : Tool
    {
        List<Vector2> _vertices = new List<Vector2>();
        Entity _entity;
        public ToolPolygon(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Enable()
        {
            base.Enable();
            _entity = new Entity(Controller.Level);
            _entity.IsPortalable = true;
        }

        public override void Disable()
        {
            _vertices.Clear();
            Controller.Level.RemoveEntity(_entity);
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
                    EditorEntity entity = Controller.CreateLevelEntity();
                    Vector2 average = new Vector2(_vertices.Average(item => item.X), _vertices.Average(item => item.Y));
                    for (int i = 0; i < _vertices.Count; i++)
                    {
                        _vertices[i] -= average;
                    }
                    Transform2D transform = entity.GetTransform();
                    transform.Position = average;
                    EntityFactory.CreateEntityPolygon(entity.Entity, transform, _vertices.ToArray());
                    entity.Entity.IsPortalable = true;
                    entity.Entity.Models[0].Wireframe = true;
                    entity.Entity.Models.Add(ModelFactory.CreatePolygon(_vertices.ToArray()));
                    entity.Entity.Models[1].SetColor(new Vector3(0.5f, 0.5f, 0.5f));
                    entity.Entity.Models[1].SetShader("default");
                    _vertices.Clear();
                    _entity.Models.Clear();
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
            _entity.Models.Clear();
            if (_vertices.Count() >= 2)
            {
                PolyCoord[] intersects = MathExt.GetLineStripIntersections(_vertices.ToArray(), true);
                Vector3[] colors = new Vector3[_vertices.Count() - 1];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = new Vector3(0, 0.5f, 0);
                }
                foreach (PolyCoord p in intersects)
                {
                    colors[p.LineIndex] = new Vector3(1f, 0.2f, 0.2f);
                }
                Model model = ModelFactory.CreateLineStrip(_vertices.ToArray(), colors);
                model.Transform.Position = new Vector3(0, 0, 6);
                model.SetShader("default");
                _entity.Models.Add(model);
            }
        }

        public override Tool Clone()
        {
            throw new NotImplementedException();
        }
    }
}

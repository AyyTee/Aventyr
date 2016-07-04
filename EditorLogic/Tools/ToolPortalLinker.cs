using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace EditorLogic
{
    public class ToolPortalLinker : Tool
    {
        Doodad line;
        EditorPortal _portalPrevious;

        public ToolPortalLinker(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
            if (_input.KeyPress(Key.Delete) || _input.KeyPress(Key.Escape) || _input.MousePress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (_input.MousePress(MouseButton.Left))
            {
                Vector2 mousePos = Controller.GetMouseWorld();
                EditorPortal portal = (EditorPortal)Controller.GetNearestObject(mousePos,
                    item => item.GetType() == typeof(EditorPortal) && (mousePos - item.GetWorldTransform().Position).Length < 1);
                if (portal != null && portal != _portalPrevious)
                {
                    if (_portalPrevious == null)
                    {
                        _portalPrevious = portal;
                    }
                    else
                    {
                        portal.Linked = _portalPrevious;
                        _portalPrevious.Linked = portal;

                        Transform2 t = portal.GetTransform();
                        if (_input.KeyDown(InputExt.KeyBoth.Control))
                        {
                            /*portal.IsMirrored = true;
                            _portalPrevious.IsMirrored = false;*/
                            /*t.Size *= -1;
                            t.IsMirrored = true;
                            portal.SetTransform(t);*/
                            portal.Transform.MirrorX = true;
                            portal.Transform.Size = -Math.Abs(portal.Transform.Size);
                            _portalPrevious.Transform.MirrorX = false;
                            _portalPrevious.Transform.Size = Math.Abs(_portalPrevious.Transform.Size);
                            //t = _portalPrevious.GetTransform();
                            //t.IsMirrored = false;
                            //_portalPrevious.SetTransform(t);
                        }
                        else
                        {
                            /*portal.IsMirrored = false;
                            _portalPrevious.IsMirrored = false;*/
                            /*t.IsMirrored = false;
                            portal.SetTransform(t);
                            t = _portalPrevious.GetTransform();
                            t.IsMirrored = false;
                            _portalPrevious.SetTransform(t);*/
                            portal.Transform.MirrorX = true;
                            portal.Transform.Size = Math.Abs(portal.Transform.Size);
                            _portalPrevious.Transform.MirrorX = true;
                            _portalPrevious.Transform.Size = Math.Abs(_portalPrevious.Transform.Size);
                        }
                        
                        
                        _portalPrevious = null;
                        Controller.SetTool(null);
                    }
                }
            }
            if (_portalPrevious != null)
            {
                line.Models.Clear();
                Model lineModel = Game.ModelFactory.CreateLineStrip(new Vector2[] {
                    Controller.GetMouseWorld(),
                    _portalPrevious.GetWorldTransform().Position
                });
                lineModel.SetColor(new Vector3(0.1f, 0.7f, 0.1f));
                line.Models.Add(lineModel);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _portalPrevious = null;
            line = new Doodad(Controller.Level);
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(line);
        }
    }
}

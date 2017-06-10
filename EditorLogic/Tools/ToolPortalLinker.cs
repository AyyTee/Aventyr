using System;
using Game;
using Game.Common;
using Game.Models;
using OpenTK;
using OpenTK.Input;
using Game.Rendering;
using OpenTK.Graphics;

namespace EditorLogic.Tools
{
    public class ToolPortalLinker : Tool
    {
        Doodad _line;
        EditorPortal _portalPrevious;

        public ToolPortalLinker(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
            if (Input.ButtonPress(Key.Delete) || Input.ButtonPress(Key.Escape) || Input.ButtonPress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (Input.ButtonPress(MouseButton.Left))
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
                        if (Input.ButtonDown(KeyBoth.Control))
                        {
                            portal.Transform = portal.Transform
                                .WithSize(-Math.Abs(portal.Transform.Size))
                                .WithMirrorX(true);
                            _portalPrevious.Transform = _portalPrevious.Transform
                                .WithSize(Math.Abs(_portalPrevious.Transform.Size))
                                .WithMirrorX(false);
                        }
                        else
                        {
                            portal.Transform = portal.Transform
                                .WithSize(Math.Abs(portal.Transform.Size))
                                .WithMirrorX(true);
                            _portalPrevious.Transform = _portalPrevious.Transform
                                .WithSize(Math.Abs(_portalPrevious.Transform.Size))
                                .WithMirrorX(true);
                        }
                        
                        
                        _portalPrevious = null;
                        Controller.SetTool(null);
                    }
                }
            }
            if (_portalPrevious != null)
            {
                _line.Models.Clear();
                Model lineModel = Game.Rendering.ModelFactory.CreateLineStrip(new Vector2[] {
                    Controller.GetMouseWorld(),
                    _portalPrevious.GetWorldTransform().Position
                });
                lineModel.SetColor(new Color4(0.1f, 0.7f, 0.1f, 1f));
                _line.Models.Add(lineModel);
            }
        }

        public override void Enable()
        {
            base.Enable();
            _portalPrevious = null;
            _line = new Doodad("Portal Linker Line");
            Controller.Level.Doodads.Add(_line);
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_line);
        }
    }
}

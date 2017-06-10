﻿using EditorLogic.Command;
using Game;
using Game.Common;
using Game.Models;
using Game.Rendering;
using OpenTK;
using OpenTK.Input;

namespace EditorLogic.Tools
{
    public class ToolAddEntity : Tool
    {
        Doodad _mouseFollow;

        public ToolAddEntity(ControllerEditor controller)
            : base(controller)
        {
        }

        public override void Update()
        {
            base.Update();
            if (_mouseFollow != null)
            {
                _mouseFollow.WorldTransform = _mouseFollow.WorldTransform.WithPosition(Controller.GetMouseWorld());
            }
            if (Input.ButtonPress(Key.Delete) || Input.ButtonPress(Key.Escape) || Input.ButtonPress(MouseButton.Right))
            {
                Controller.SetTool(null);
            }
            else if (Input.ButtonPress(MouseButton.Left))
            {
                EditorEntity editorEntity = new EditorEntity(Controller.Level);
                Model m = Game.Rendering.ModelFactory.CreateCube();
                m.SetTexture(Controller.Window.Textures.@Default);
                editorEntity.AddModel(m);
                editorEntity.Name = "Editor Entity";

                editorEntity.SetTransform(editorEntity.GetTransform().WithPosition(Controller.GetMouseWorld()));
                Controller.Selection.Set(editorEntity);

                AddEntity command = new AddEntity(Controller, editorEntity);
                Controller.StateList.Add(command, true);

                if (!Input.ButtonDown(KeyBoth.Shift))
                {
                    Controller.SetTool(null);
                }
            }
        }

        public override void Enable()
        {
            base.Enable();
            _mouseFollow = new Doodad("Add Entity");
            Controller.Level.Doodads.Add(_mouseFollow);
            _mouseFollow.Models.Add(Game.Rendering.ModelFactory.CreateCube());
            _mouseFollow.Models[0].SetTexture(Controller.Window.Textures.@Default);
            _mouseFollow.IsPortalable = true;
        }

        public override void Disable()
        {
            base.Disable();
            Controller.Level.Doodads.Remove(_mouseFollow);
        }
    }
}

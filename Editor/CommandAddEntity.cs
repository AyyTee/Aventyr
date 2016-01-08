using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class CommandAddEntity : CommandAdd
    {
        public CommandAddEntity(ControllerEditor controller, Transform2D transform)
            : base(controller, transform)
        {
        }

        public override void Do()
        {
            base.Do();
            EditorEntity editorEntity = _controller.CreateLevelEntity();
            _editorObject = editorEntity;
            editorEntity.Entity.AddModel(ModelFactory.CreateCube());
            editorEntity.Entity.ModelList[0].SetTexture(Renderer.Textures["default.png"]);
            editorEntity.Entity.IsPortalable = true;
            editorEntity.SetTransform(_transform);
            _controller.selection.Set(editorEntity);
        }

        public override ICommand Clone()
        {
            CommandAddEntity clone = new CommandAddEntity(_controller, _transform);
            clone._editorObject = _editorObject;
            return clone;
        }
    }
}

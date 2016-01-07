using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class CommandAddEntity : ICommand
    {
        readonly Transform2D _transform;
        readonly ControllerEditor _controller;
        EditorEntity _editorEntity;

        HashSet<EditorObject> _modified = new HashSet<EditorObject>();

        public CommandAddEntity(ControllerEditor controller, Transform2D transform)
        {
            _transform = transform;
            _controller = controller;
        }

        public void Do()
        {
            _editorEntity = new EditorEntity(_controller);
            _editorEntity.SetTransform(_transform);
            _editorEntity.Entity.AddModel(ModelFactory.CreateCube());
            _editorEntity.Entity.ModelList[0].SetTexture(Renderer.Textures["default.png"]);
            _editorEntity.Entity.IsPortalable = true;
            _controller.selection.Set(_editorEntity);
        }

        public void Redo()
        {
            _editorEntity.SetParent(_editorEntity.Scene.Root);
            _controller.selection.Set(_editorEntity);
        }

        public void Undo()
        {
            _editorEntity.Remove();
        }

        public ICommand DeepClone()
        {
            CommandAddEntity clone = new CommandAddEntity(_controller, _transform);
            clone._editorEntity = _editorEntity;
            return clone;
        }
    }
}

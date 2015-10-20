
using System.Diagnostics;
namespace Game
{
    public abstract class Placeable2D
    {
        private Transform2D _transform = null;
        public Transform2D Transform
        {
            get { return _transform; }
            set 
            {
                Debug.Assert(_transform == null, "Transform can only be assigned once.");
                _transform = value; 
            }
        }

        private Scene _scene = null;
        public Scene Scene
        {
            get { return _scene; }
            set 
            {
                Debug.Assert(_scene == null, "This has already been assigned to a scene.");
                _scene = value; 
            }
        }
    }
}

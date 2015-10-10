
namespace Game
{
    public abstract class Placeable2D
    {
        private Transform2D _transform = new Transform2D();

        public Transform2D Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }
    }
}

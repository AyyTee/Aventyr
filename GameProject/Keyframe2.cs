using OpenTK;

namespace Game
{
    public class Keyframe2
    {
        public Keyframe X;
        public Keyframe Y;

        public Keyframe2(float time, Vector2 value)
        {
            X = new Keyframe(time, value.X);
            Y = new Keyframe(time, value.Y);
        }
    }
}

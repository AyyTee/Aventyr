using OpenTK;
using System.Runtime.Serialization;
using Game.Serialization;

namespace Game.Animation
{
    [DataContract]
    public class Curve2 : IShallowClone<Curve2>
    {
        [DataMember]
        Curve _x;
        [DataMember]
        Curve _y;
        public Vector2 DefaultValue
        {
            get { return new Vector2(_x.DefaultValue, _y.DefaultValue); }
            set
            {
                _x.DefaultValue = value.X;
                _y.DefaultValue = value.Y;
            }
        }
        public bool IsLoop
        {
            get { return _x.IsLoop; }
            set
            {
                _x.IsLoop = value;
                _y.IsLoop = value;
            }
        }
        [DataMember]
        public string Name;

        public Curve2()
            : this(Vector2.Zero)
        {
        }

        public Curve2(Vector2 defaultValue)
        {
            _x = new Curve(defaultValue.X);
            _y = new Curve(defaultValue.Y);
        }

        public void AddKeyframe(Keyframe2 keyframe)
        {
            _x.AddKeyframe(keyframe.X);
            _y.AddKeyframe(keyframe.Y);
        }

        public Vector2 GetValue(float time)
        {
            Vector2 result;
            result.X = _x.GetValue(time);
            result.Y = _y.GetValue(time);
            return result;
        }

        public Vector2 GetDerivative(float time)
        {
            Vector2 result;
            result.X = _x.GetDerivative(time);
            result.Y = _y.GetDerivative(time);
            return result;
        }

        public Curve2 ShallowClone()
        {
            Curve2 clone = new Curve2();
            clone._x = _x.ShallowClone();
            clone._y = _y.ShallowClone();
            return clone;
        }

        /*public SortedList<float, Keyframe2> GetKeyframes()
        {
            SortedList<float, Keyframe2> keyframes = new SortedList<float, Keyframe2>();
            keyframes.Add
        }*/

        /*public static Model GetModel(Curve2 fCurve)
        {
            Mesh mesh = new Mesh();
            Line[] line = new Line[fCurve.Keyframe.X.Count - 1];
            for (int i = 0; i < line.Length; i++)
            {
                line[i] = new Line(fCurve.Keyframes[i].)
            }
            ModelFactory.AddLines(mesh, )

            Model model = new Model();
        }*/
    }
}

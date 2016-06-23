using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class Curve2
    {
        [DataMember]
        Curve X;
        [DataMember]
        Curve Y;
        public Vector2 DefaultValue
        {
            get { return new Vector2(X.DefaultValue, Y.DefaultValue); }
            set
            {
                X.DefaultValue = value.X;
                Y.DefaultValue = value.Y;
            }
        }
        public bool IsLoop
        {
            get { return X.IsLoop; }
            set
            {
                X.IsLoop = value;
                Y.IsLoop = value;
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
            X = new Curve(defaultValue.X);
            Y = new Curve(defaultValue.Y);
        }

        public void AddKeyframe(Keyframe2 keyframe)
        {
            X.AddKeyframe(keyframe.X);
            Y.AddKeyframe(keyframe.Y);
        }

        public Vector2 GetValue(float time)
        {
            Vector2 result;
            result.X = X.GetValue(time);
            result.Y = Y.GetValue(time);
            return result;
        }

        public Vector2 GetDerivative(float time)
        {
            Vector2 result;
            result.X = X.GetDerivative(time);
            result.Y = Y.GetDerivative(time);
            return result;
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

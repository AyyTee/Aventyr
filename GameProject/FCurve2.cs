using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class FCurve2
    {
        [DataMember]
        FCurve X;
        [DataMember]
        FCurve Y;
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

        public FCurve2()
            : this(Vector2.Zero)
        {
        }

        public FCurve2(Vector2 defaultValue)
        {
            X = new FCurve(defaultValue.X);
            Y = new FCurve(defaultValue.Y);
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
    }
}

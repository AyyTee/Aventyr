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
    public class FCurveTransform2
    {
        [DataMember]
        public FCurve2 PosCurve = new FCurve2();
        [DataMember]
        public FCurve RotCurve = new FCurve();
        [DataMember]
        public FCurve SizeCurve = new FCurve();
        [DataMember]
        public bool MirrorX;

        public FCurveTransform2()
        {
        }

        public Transform2 GetTransform(float time)
        {
            return new Transform2(PosCurve.GetValue(time), SizeCurve.GetValue(time), RotCurve.GetValue(time), MirrorX);
        }

        public Transform2 GetVelocity(float time)
        {
            return new Transform2(PosCurve.GetDerivative(time), SizeCurve.GetDerivative(time), RotCurve.GetDerivative(time), false);
        }

        public void AddKeyframe(float time, Transform2 keyframe)
        {
            PosCurve.AddKeyframe(new Keyframe2(time, keyframe.Position));
            RotCurve.AddKeyframe(new Keyframe(time, keyframe.Rotation));
            SizeCurve.AddKeyframe(new Keyframe(time, keyframe.Size));
        }
    }
}

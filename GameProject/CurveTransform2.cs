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
    public class CurveTransform2
    {
        [DataMember]
        public Curve2 PosCurve = new Curve2();
        [DataMember]
        public Curve RotCurve = new Curve();
        [DataMember]
        public Curve SizeCurve = new Curve();
        [DataMember]
        public bool MirrorX;
        [DataMember]
        public Transform2 Offset = new Transform2();

        public CurveTransform2()
        {
        }

        public Transform2 GetTransform(float time)
        {
            return new Transform2(PosCurve.GetValue(time), SizeCurve.GetValue(time), RotCurve.GetValue(time), MirrorX).Transform(Offset);
        }

        public Transform2 GetVelocity(float time)
        {
            return new Transform2(PosCurve.GetDerivative(time), SizeCurve.GetDerivative(time), RotCurve.GetDerivative(time), false).Transform(Offset);
        }

        public void AddKeyframe(float time, Transform2 keyframe)
        {
            PosCurve.AddKeyframe(new Keyframe2(time, keyframe.Position));
            RotCurve.AddKeyframe(new Keyframe(time, keyframe.Rotation));
            SizeCurve.AddKeyframe(new Keyframe(time, keyframe.Size));
        }
    }
}

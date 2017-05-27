using Game.Portals;
using OpenTK;
using System.Runtime.Serialization;
using Game.Common;
using Game.Serialization;
using Game.Rendering;

namespace Game.Animation
{
    [DataContract]
    public class CurveTransform2 : IShallowClone<CurveTransform2>
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
        public Transform2 TransformOffset = new Transform2();
        [DataMember]
        public Transform2 VelocityOffset = Transform2.CreateVelocity();

        public CurveTransform2()
        {
        }

        public Transform2 GetTransform(float time)
        {
            return new Transform2(
                PosCurve.GetValue(time), 
                SizeCurve.GetValue(time), 
                RotCurve.GetValue(time), 
                MirrorX).Transform(TransformOffset);
        }

        public Transform2 GetVelocity(float time)
        {
            Transform2 offset = TransformOffset.SetPosition(Vector2.Zero);
            Transform2 velocity = new Transform2(
                PosCurve.GetDerivative(time), 
                SizeCurve.GetDerivative(time), 
                RotCurve.GetDerivative(time), 
                false);
            Transform2 velocityTransformed = velocity.Transform(offset);
            //velocityTransformed.Rotation = velocity.Rotation;
            return velocityTransformed;
        }

        public void EnterPortal(IPortalRenderable enter, IPortalRenderable exit)
        {
            TransformOffset = TransformOffset.Transform(Portal.GetLinkedTransform(enter, exit));
            //TransformVelocity = 
        }

        public void AddKeyframe(float time, Transform2 keyframe)
        {
            PosCurve.AddKeyframe(new Keyframe2(time, keyframe.Position));
            RotCurve.AddKeyframe(new Keyframe(time, keyframe.Rotation));
            SizeCurve.AddKeyframe(new Keyframe(time, keyframe.Size));
        }

        public CurveTransform2 ShallowClone()
        {
            CurveTransform2 clone = new CurveTransform2();
            clone.PosCurve = PosCurve.ShallowClone();
            clone.RotCurve = RotCurve.ShallowClone();
            clone.SizeCurve = SizeCurve.ShallowClone();
            clone.MirrorX = MirrorX;
            clone.TransformOffset = TransformOffset.ShallowClone();
            clone.VelocityOffset = VelocityOffset.ShallowClone();
            return clone;
        }
    }
}

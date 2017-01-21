using Game.Portals;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Game.Common;

namespace Game.Animation
{
    [DataContract]
    public class AnimationDriver : ISceneObject, IStep
    {
        [DataMember]
        public Dictionary<IPortalable, CurveTransform2> Animated = new Dictionary<IPortalable, CurveTransform2>();

        [DataMember]
        public string Name { get; set; } = nameof(AnimationDriver);

        public AnimationDriver()
        {
        }

        public void Add(IPortalable portalable, CurveTransform2 curve)
        {
            Animated.Add(portalable, curve.ShallowClone());
            portalable.EnterPortal += (data, transformPrev, velocityPrev) => {
                Animated[portalable].EnterPortal(data.EntrancePortal, data.EntrancePortal.Linked);
            };
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            foreach (IPortalable p in Animated.Keys)
            {
                Transform2 velocity = Animated[p].GetVelocity((float)(scene.Time));
                p.SetVelocity(velocity);
                Transform2 t = Animated[p].GetTransform((float)(scene.Time));
                p.SetTransform(t);
            }
        }

        public void StepEnd(IScene scene, float stepSize)
        {
        }

        public void Remove()
        {

        }
    }
}

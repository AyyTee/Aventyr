using Game.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game.Animation
{
    [DataContract]
    public class AnimationDriver : ISceneObject, IStep
    {
        [DataMember]
        public Dictionary<IPortalable, CurveTransform2> animated = new Dictionary<IPortalable, CurveTransform2>();

        public AnimationDriver()
        {
        }

        public void Add(IPortalable portalable, CurveTransform2 curve)
        {
            animated.Add(portalable, curve.ShallowClone());
            portalable.EnterPortal += (data, transformPrev, velocityPrev) => {
                animated[portalable].EnterPortal(data.EntrancePortal, data.EntrancePortal.Linked);
            };
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            foreach (IPortalable p in animated.Keys)
            {
                Transform2 velocity = animated[p].GetVelocity((float)(scene.Time));
                p.SetVelocity(velocity);
                Transform2 t = animated[p].GetTransform((float)(scene.Time));
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

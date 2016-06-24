using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class AnimationDriver : ISceneObject, IStep
    {
        public Dictionary<IPortalable, CurveTransform2> animated = new Dictionary<IPortalable, CurveTransform2>();

        public AnimationDriver()
        {
        }

        public void Add(IPortalable portalable, CurveTransform2 curve)
        {
            animated.Add(portalable, curve);
            portalable.enterPortal += (portal, transformPrev, velocityPrev) => {
                //Portal.GetPortalMatrix
                Transform2 t = animated[portalable].Offset.Transform(Portal.GetPortalTransform(portal));
                animated[portalable].Offset = t;
            };
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            foreach (IPortalable p in animated.Keys)
            {
                Transform2 velocity = animated[p].GetVelocity((float)(scene.Time));
                p.SetVelocity(velocity);
            }
        }

        public void StepEnd(IScene scene, float stepSize)
        {
        }
    }
}

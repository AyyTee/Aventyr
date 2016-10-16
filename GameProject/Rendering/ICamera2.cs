using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ICamera2 : ISceneObject
    {
        Transform2 GetWorldTransform(bool ignorePortals = false);
        Transform2 GetWorldVelocity(bool ignorePortals = false);
        float Aspect { get; }
        /// <summary>
        /// View offset in clip space coordinates [-1,1].
        /// </summary>
        Vector2 ViewOffset { get; }
        /// <summary>
        /// Field of view in radians.  Only affects perspective view matrices, not orthographic view matrices.
        /// </summary>
        double Fov { get; }
        float ZNear { get; }
        float ZFar { get; }
    }
}

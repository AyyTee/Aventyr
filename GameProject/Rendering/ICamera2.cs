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
        Matrix4 GetViewMatrix(bool isOrtho = true);
        Transform2 GetWorldTransform(bool ignorePortals = false);
        Transform2 GetWorldVelocity(bool ignorePortals = false);
        float Aspect { get; }
        /// <summary>Focal point offset.</summary>
        Vector2 ViewOffset { get; }
        /// <summary>
        /// Field of view.  Only affects perspective view matrices, not orthographic view matrices.
        /// </summary>
        double Fov { get; }
        float ZNear { get; }
        float ZFar { get; }
    }
}

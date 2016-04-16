using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ICamera2
    {
        Matrix4 GetViewMatrix(bool isOrtho = true);
        Transform2 GetWorldTransform();
        Transform2 GetWorldVelocity();
        float Aspect { get; set; }
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

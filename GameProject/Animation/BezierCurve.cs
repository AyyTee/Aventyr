using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Animation
{
    public class BezierCurve
    {
        public Vector2[] controlPoints;

        public BezierCurve(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            controlPoints = new Vector2[]
            {
                controlPoints[0],
                controlPoints[1],
                controlPoints[2],
                controlPoints[3]
            };
        }

        public Vector2 GetPoint(float t)
        {
            return (
                controlPoints[0] * (float)Math.Pow((1 - t), 3) +
                controlPoints[1] * (float)(3 * t * Math.Pow((1 - t), 2)) +
                controlPoints[2] * (float)(3 * Math.Pow(t, 2) * (1 - t)) +
                controlPoints[3] * (float)Math.Pow(t, 3)
            );
        }
    }
}

using OpenTK;
using System;

namespace Game.Animation
{
    public class BezierCurve
    {
        public Vector2[] ControlPoints;

        public BezierCurve(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            ControlPoints = new Vector2[]
            {
                ControlPoints[0],
                ControlPoints[1],
                ControlPoints[2],
                ControlPoints[3]
            };
        }

        public Vector2 GetPoint(float t)
        {
            return (
                ControlPoints[0] * (float)Math.Pow((1 - t), 3) +
                ControlPoints[1] * (float)(3 * t * Math.Pow((1 - t), 2)) +
                ControlPoints[2] * (float)(3 * Math.Pow(t, 2) * (1 - t)) +
                ControlPoints[3] * (float)Math.Pow(t, 3)
            );
        }
    }
}

using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class ActorExt
    {
        /// <summary>
        /// Returns vertices defining the collision mask contour (Not the fixtures themselves!)
        /// </summary>
        public static List<Vector2> GetFixtureContour(IActor actor)
        {
            return GetFixtureContour(actor.Vertices, actor.GetTransform());
        }

        public static List<Vector2> GetFixtureContour(IList<Vector2> vertices, Transform2 transform)
        {
            Matrix4 scaleMat = Matrix4.CreateScale(new Vector3(transform.Scale));
            return (List<Vector2>)Vector2Ext.Transform(vertices, scaleMat);
        }
    }
}

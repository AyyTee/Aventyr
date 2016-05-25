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
        /// Returns polygon that is the local polygon with only the local transforms Scale component applied. 
        /// This is useful because the vertices should exactly match up with vertices in the physics fixtures for this Actor's body.
        /// </summary>
        public static List<Vector2> GetFixtureContour(IActor actor)
        {
            return GetFixtureContour(actor.Vertices, actor.GetTransform().Scale);
        }

        public static List<Vector2> GetFixtureContour(IList<Vector2> vertices, Vector2 scale)
        {
            Matrix4 scaleMat = Matrix4.CreateScale(new Vector3(scale));
            return (List<Vector2>)Vector2Ext.Transform(vertices, scaleMat);
        }
    }
}

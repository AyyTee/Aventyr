using ClipperLib;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class ActorExt
    {
        /// <summary>
        /// Returns polygon that is the local polygon with only the local transforms Scale component applied. 
        /// This is useful because the vertices should match up with vertices in the physics fixtures for this Actor's body (within rounding errors).
        /// </summary>
        public static List<Vector2> GetFixtureContour(Actor actor)
        {
            return GetFixtureContour(actor.Vertices, actor.GetTransform().Scale);
        }

        public static List<Vector2> GetFixtureContour(IList<Vector2> vertices, Vector2 scale)
        {
            Debug.Assert(scale.X != 0 && scale.Y != 0);
            Matrix4 scaleMat = Matrix4.CreateScale(new Vector3(scale));
            List<Vector2> contour = Vector2Ext.Transform(vertices, scaleMat);
            if (Math.Sign(scale.X) != Math.Sign(scale.Y))
            {
                contour.Reverse();
            }
            return contour;
        }

        public static void AssertTransform(Actor actor)
        {
            /*Bodies don't have a scale component so we use the default scale when comparing the Actor's
             * scale to that of the child bodies.*/
            Transform2 actorTransform = actor.WorldTransform;
            actorTransform.SetScale(Vector2.One);

            foreach (BodyData data in Tree<BodyData>.GetAll(BodyExt.GetData(actor.Body)))
            {
                Transform2 bodyTransform = UndoPortalTransform(data, BodyExt.GetTransform(data.Body));
                bodyTransform.SetScale(Vector2.One);
                Debug.Assert(bodyTransform.AlmostEqual(actorTransform, 0.01f, 0.01f));
            }
        }

        private static Transform2 UndoPortalTransform(BodyData data, Transform2 transform)
        {
            Transform2 copy = transform.ShallowClone();
            if (data.Parent == null)
            {
                return copy;
            }
            return UndoPortalTransform(
                data.Parent, 
                copy.Transform(Portal.GetLinkedTransform(data.BodyParent.Portal).Inverted()));
        }

        /// <summary>
        /// Verifies the BodyType for Actor bodies is correct.
        /// </summary>
        /// <returns></returns>
        public static void AssertBodyType(Actor actor)
        {
            if (actor.Body.BodyType != actor.BodyType)
            {
                Debug.Fail("");
            }
            foreach (BodyData data in BodyExt.GetData(actor.Body).Children)
            {
                _assertBodyType(data);
            }
        }

        private static void _assertBodyType(BodyData bodyData)
        {
            Debug.Assert(
                (bodyData.Body.BodyType == BodyType.Dynamic && bodyData.Actor.BodyType == BodyType.Dynamic) ||
                (bodyData.Body.BodyType == BodyType.Kinematic && bodyData.Actor.BodyType != BodyType.Dynamic));
            foreach (BodyData data in bodyData.Children)
            {
                _assertBodyType(data);
            }
        }
    }
}

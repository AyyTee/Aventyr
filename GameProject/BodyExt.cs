using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Game
{
    public static class BodyExt
    {
        public static Body CreateBody(World world)
        {
            Body body = new Body(world);
            world.ProcessChanges();
            return body;
        }

        /*public static Transform2D GetTransform(Body body)
        {
            var transform = new FarseerPhysics.Common.Transform();
            body.GetTransform(out transform);
            return new Transform2D(transform.Position, transform.Angle);
        }*/

        public static BodyUserData SetUserData(Body body, Actor entity)
        {
            //Ugly solution to storing Game classes in a way that still works when deserializing the data.
            //This list is intended to only store one element.
            var a = new List<BodyUserData>();
            body.UserData = a;
            a.Add(new BodyUserData(entity, body));
            return a[0];
        }

        public static BodyUserData GetUserData(Body body)
        {
            Debug.Assert(body != null);
            return ((List<BodyUserData>)body.UserData)[0];
        }

        public static Transform2D GetTransform(Body body)
        {
            return new Transform2D(Vector2Ext.ConvertTo(body.Position), body.Rotation);
        }

        public static void SetTransform(Body body, Transform2D transform)
        {
            body.SetTransform(Vector2Ext.ConvertToXna(transform.Position), transform.Rotation);
        }

        /// <summary>
        /// Returns the area of non-portal fixtures that are in the same coordinate space as localPoint. 
        /// In other words, find the area not on other other side of a portal relative to localPoint.
        /// </summary>
        public static float GetLocalMass(Body body, Vector2 localPoint)
        {
            //Transform2D bodyTransform = GetTransform(body);
            FarseerPhysics.Common.Transform bodyTransform;
            body.GetTransform(out bodyTransform);
            Vector3 offset = new Vector3(-body.Position.X, -body.Position.Y, 0);
            Matrix4 bodyMatrix = Matrix4.CreateTranslation(offset);//bodyTransform.GetMatrix().Inverted();
            float totalMass = body.Mass;
            foreach (Fixture f in body.FixtureList)
            {
                FixtureUserData userData = FixtureExt.GetUserData(f);
                float area = 0;
                foreach (FixturePortal portal in userData.PortalCollisions)
                {
                    Vector2[] verts = portal.GetWorldVerts();
                    //verts = Vector2Ext.Transform(verts, bodyMatrix);
                    Line line = new Line(verts);
                    Vector2 normal = line.GetNormal();
                    if (line.GetSideOf(localPoint) == Line.Side.IsLeftOf)
                    {
                        normal = -normal;
                    }

                    Xna.Vector2 v, xnaNormal;
                    xnaNormal = Vector2Ext.ConvertToXna(normal);
                    area += f.Shape.ComputeSubmergedArea(ref xnaNormal, line.GetOffset(), ref bodyTransform, out v);
                }
                totalMass -= f.Shape.Density * area;
            }
            return totalMass;
        }

        public static float GetLocalMass(Body body, Xna.Vector2 localPoint)
        {
            return GetLocalMass(body, Vector2Ext.ConvertTo(localPoint));
        }

        public static void Mirror(Body body, bool xMirror, bool yMirror)
        {
            foreach (Fixture f in body.FixtureList)
            {
                Shape mirrorShape = null;
                switch (f.Shape.ShapeType)
                {
                    case ShapeType.Polygon:
                        break;

                    case ShapeType.Edge:
                        EdgeShape mirrorTemp = (EdgeShape)mirrorShape;
                        break;

                    case ShapeType.Circle:
                        mirrorShape = f.Shape.Clone();
                        break;
                }
                //Shape mirrorShape = new Shape();
                //Fixture mirrorFixture = new Fixture(body, )
            }
        }
    }
}

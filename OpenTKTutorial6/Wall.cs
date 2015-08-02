using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using Xna = Microsoft.Xna.Framework;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;
using OpenTK;

namespace Game
{
    class Wall : Polygon
    {
        GameWorld GameWorld;
        Body Body;
        public Wall(GameWorld GameWorld)
        {
            this.GameWorld = GameWorld;
            Body = BodyFactory.CreateBody(GameWorld.PhysWorld, this);
            Body.BodyType = BodyType.Static;
        }
        public new void BufferVertices(BufferUsageHint Hint)
        {
            base.BufferVertices(Hint);
            for (int i = 0; i < GeometryTriangles.Count(); i++)
            {
                Vertices Poly;
                float X;
                float Y;
                for (int j = 0; j < GeometryTriangles[i].Triangles.Count(); j++ )
                {
                    Poly = new Vertices(3);
                    X = (float)GeometryTriangles[i].Triangles[j].Points[0].X;
                    Y = (float)GeometryTriangles[i].Triangles[j].Points[0].Y;
                    Poly.Add(new Xna.Vector2(X, Y));
                    X = (float)GeometryTriangles[i].Triangles[j].Points[1].X;
                    Y = (float)GeometryTriangles[i].Triangles[j].Points[1].Y;
                    Poly.Add(new Xna.Vector2(X, Y));
                    X = (float)GeometryTriangles[i].Triangles[j].Points[2].X;
                    Y = (float)GeometryTriangles[i].Triangles[j].Points[2].Y;
                    Poly.Add(new Xna.Vector2(X, Y));
                    if (Poly.IsCounterClockWise() == false)
                    {
                        Poly.Reverse();
                    }
                    PolygonShape Shape = new PolygonShape(Poly, 1);
                    new Fixture(Body, Shape);
                }
                GameWorld.PhysWorld.ProcessChanges();
            }
        }
    }
}

using OpenTK;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class ModelFactory
    {
        public static Model CreatePlane(Vector2 Scale)
        {
            Vertex[] vertices = new Vertex[] {
                new Vertex(new Vector3(-0.5f * Scale.X, 0.5f * Scale.Y,  0f), new Vector2(0, 0)),
                new Vertex(new Vector3(0.5f * Scale.X, 0.5f * Scale.Y,  0f), new Vector2(1, 0)),
                new Vertex(new Vector3(0.5f * Scale.X, -0.5f * Scale.Y,  0f), new Vector2(1, 1)),
                new Vertex(new Vector3(-0.5f * Scale.X, -0.5f * Scale.Y,  0f), new Vector2(0, 1))
            };

            /*int[] indices = new int[] {
                0, 2, 1,
                0, 3, 2,
            };*/
            Model model = new Model(vertices);
            model.AddTriangle(0, 2, 1);
            model.AddTriangle(0, 3, 2);
            return model;
        }

        public static Model CreatePlane()
        {
            return CreatePlane(new Vector2(1, 1));
        }

        /*public static Model CreateCircle()
        {
            Model model = new Model();
        }*/

        public static Model CreateCube()
        {
            Vertex[] vertices = new Vertex[] {
                //left
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                //back
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                //right
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                //top
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                //front
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(0.0f, 1.0f)), 
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(1.0f, 0.0f)), 
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(1.0f, 1.0f)),
                //bottom
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 1.0f)), 
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(0.0f, 1.0f))
            };

            int[] indices = new int[] {
                //left
                0,1,2,0,3,1,
                //back
                4,5,6,4,6,7,
                //right
                8,9,10,8,10,11,
                //top
                13,14,12,13,15,14,
                //front
                16,17,18,16,19,17,
                //bottom 
                20,21,22,20,22,23
            };
            
            Model model = new Model(vertices);
            model.AddTriangles(indices);
            //model.SetTexture(Renderer.Textures["default.png"]);
            return model;
        }

        /// <summary>
        /// Create a polygon model from an array of vertices. If the polygon is degenerate or non-simple then the model will be empty.
        /// </summary>
        public static Model CreatePolygon(Vector2[] vertices)
        {
            return CreatePolygon(PolygonFactory.CreatePolygon(vertices), new Vector3());
        }

        /// <summary>
        /// Create a polygon model from an array of vertices. If the polygon is degenerate or non-simple then the model will be empty.
        /// </summary>
        public static Model CreatePolygon(Vector2[] vertices, Vector3 offset)
        {
            return CreatePolygon(PolygonFactory.CreatePolygon(vertices), offset);
        }

        public static Model CreatePolygon(Polygon polygon)
        {
            return CreatePolygon(polygon, new Vector3());
        }

        public static Model CreatePolygon(Polygon polygon, Vector3 offset)
        {
            Model model = new Model();
            if (polygon == null)
            {
                return model;
            }
            Vertex[] verts = new Vertex[polygon.Points.Count];
            List<int> indices = new List<int>();

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                TriangulationPoint p = polygon.Points[i];
                Vector3 v = new Vector3((float)p.X, (float)p.Y, 0) + offset;
                float tx = (float)((p.X - polygon.MinX) / (polygon.MaxX - polygon.MinX));
                float ty = (float)((p.Y - polygon.MinY) / (polygon.MaxY - polygon.MinY));
                Vector2 tc = new Vector2(tx, ty);
                //verts[i] = new Vertex(v, tc);
                model.Vertices.Add(new Vertex(v, tc));
            }

            foreach (Poly2Tri.DelaunayTriangle t in polygon.Triangles)
            {
                int index0, index1, index2;
                index0 = polygon.IndexOf(t.Points._0);
                index1 = polygon.IndexOf(t.Points._1);
                index2 = polygon.IndexOf(t.Points._2);
                //Sometimes the index is -1 and I don't know why. Ignore those points.
                if (index0 < 0 || index1 < 0 || index2 < 0)
                {
                    continue;
                }
                model.AddTriangle(index0, index1, index2);
            }

            return model;
        }

        public static Model CreateLines(Line[] lines)
        {
            Model model = new Model();
            model.Wireframe = true;
            for (int i = 0; i < lines.Length; i++)
            {
                Vector3 v;
                v = new Vector3(lines[i][0].X, lines[i][0].Y, 0);
                int index0 = model.AddVertex(new Vertex(v, new Vector2()));
                v = new Vector3(lines[i][1].X, lines[i][1].Y, 0);
                int index1 = model.AddVertex(new Vertex(v, new Vector2()));
                model.AddTriangle(index0, index1, index1);
            }
            return model;
        }

        public static Model CreateLinesWidth(Line[] lines, float width)
        {
            Model model = new Model();
            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 vStart, vEnd;
                vStart = lines[i][0];
                vEnd = lines[i][1];
                CreateLineWidth(model, vStart, vEnd, width);
                /*Vector2 offset = (vStart - vEnd).PerpendicularLeft.Normalized() * width / 2;

                int index0 = model.AddVertex(new Vertex(new Vector3(vStart + offset), new Vector2()));
                int index1 = model.AddVertex(new Vertex(new Vector3(vStart - offset), new Vector2()));
                int index2 = model.AddVertex(new Vertex(new Vector3(vEnd - offset), new Vector2()));
                int index3 = model.AddVertex(new Vertex(new Vector3(vEnd + offset), new Vector2()));
                model.AddTriangle(index0, index1, index2);
                model.AddTriangle(index0, index3, index2);*/
            }
            return model;
        }

        public static Model CreateLineStripWidth(Vector2[] vertices, float width, bool closed)
        {
            Model model = new Model();
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                CreateLineWidth(model, vertices[i], vertices[i + 1], width);
            }
            if (closed)
            {
                CreateLineWidth(model, vertices[vertices.Length - 1], vertices[0], width);
            }
            return model;
        }

        private static void CreateLineWidth(Model model, Vector2 v0, Vector2 v1, float width)
        {
            Vector2 offset = (v0 - v1).PerpendicularLeft.Normalized() * width / 2;
            int index0 = model.AddVertex(new Vertex(new Vector3(v0 + offset), new Vector2()));
            int index1 = model.AddVertex(new Vertex(new Vector3(v0 - offset), new Vector2()));
            int index2 = model.AddVertex(new Vertex(new Vector3(v1 - offset), new Vector2()));
            int index3 = model.AddVertex(new Vertex(new Vector3(v1 + offset), new Vector2()));
            model.AddTriangle(index0, index1, index2);
            model.AddTriangle(index0, index3, index2);
        }

        public static Model CreateLineStrip(Vector2[] vertices, Vector3[] colors)
        {
            Debug.Assert(vertices.Length >= 2);
            Model model = new Model();
            model.Wireframe = true;
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                Vector3 v, color = new Vector3();
                if (colors != null)
                {
                    color = colors[i];
                }
                
                v = new Vector3(vertices[i].X, vertices[i].Y, 0);
                int index0 = model.AddVertex(new Vertex(v, new Vector2(), color));
                v = new Vector3(vertices[i + 1].X, vertices[i + 1].Y, 0);
                int index1 = model.AddVertex(new Vertex(v, new Vector2(), color));
                model.AddTriangle(index0, index1, index1);
            }
            return model;
        }

        public static Model CreateLineStrip(Vector2[] vertices)
        {
            return CreateLineStrip(vertices, null);
        }

        public static Model CreateCircle(Vector3 origin, float radius, int detail)
        {
            Debug.Assert(detail >= 3, "Detail must be greater or equal to 3.");
            Model model = new Model();
            for (int i = 0; i < detail; i++)
            {
                double rad = Math.PI * 2 * i / detail;
                Vector3 pos = new Vector3((float)Math.Cos(rad), (float)Math.Sin(rad), 0) * radius + origin;
                Vector2 textureCoord = new Vector2((float)(1 + Math.Cos(rad) / 2), (float)(1 + Math.Sin(rad) / 2));
                model.Vertices.Add(new Vertex(pos, textureCoord));
            }

            for (int i = 0; i < detail - 1; i++)
            {
                if (i == detail - 1 - i || i + 1 == detail - 1 - i)
                {
                    continue;
                }
                model.AddTriangle(i, i + 1, detail - 1 - i);
            }
            return model;
        }

        /// <summary>
        /// Creates a 2 dimensional arrow.
        /// </summary>
        /// <param name="origin">Starting point of the arrow.</param>
        /// <param name="pointAt">Position of the tip of the arrow relative to the origin.</param>
        /// <param name="lineThickness">Thickness of the line.</param>
        /// <param name="arrowLength">Length of the arrow head.</param>
        /// <param name="arrowThickness">Thickness of the arrow head.</param>
        /// <returns></returns>
        public static Model CreateArrow(Vector3 origin, Vector2 pointAt, float lineThickness, float arrowLength, float arrowThickness)
        {
            float length = pointAt.Length;
            Vector2[] polygon;
            if (length <= arrowLength)
            {
                polygon = new Vector2[] {
                    new Vector2(0, arrowLength),
                    new Vector2(-arrowThickness, 0),
                    new Vector2(arrowThickness, 0),
                };
            }
            else
            {
                polygon = new Vector2[] {
                    new Vector2(0, length),
                    new Vector2(-arrowThickness, length - arrowLength),
                    new Vector2(-lineThickness/2, length - arrowLength),
                    new Vector2(-lineThickness/2, 0),
                    new Vector2(lineThickness/2, 0),
                    new Vector2(lineThickness/2, length - arrowLength),
                    new Vector2(arrowThickness, length - arrowLength),
                };
            }
            polygon = Vector2Ext.Transform(polygon, Matrix4.CreateRotationZ((float)-(MathExt.AngleVector(pointAt) + Math.PI / 2)));
            return CreatePolygon(polygon, origin);
        }
    }
}

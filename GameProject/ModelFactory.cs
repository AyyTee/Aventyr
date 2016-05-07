using ClipperLib;
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
            Mesh mesh = new Mesh();
            mesh.Vertices = vertices.ToList();
            mesh.Indices.AddRange(new[] { 0, 2, 1 });
            mesh.Indices.AddRange(new[] { 0, 3, 2 });
            Model model = new Model(mesh);
            /*model.AddTriangle(0, 2, 1);
            model.AddTriangle(0, 3, 2);*/
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
            return CreateCube(new Vector3(1,1,1));
        }

        public static Model CreateCube(Vector3 scale)
        {
            Vertex[] vertices = new Vertex[] {
                //left
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f) * scale, new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f) * scale, new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f)),
                //back
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f)),
                //right
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f) * scale, new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f) * scale, new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f)),
                //top
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f)),
                //front
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f) * scale, new Vector2(0.0f, 1.0f)), 
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f) * scale, new Vector2(1.0f, 0.0f)), 
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f) * scale, new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f) * scale, new Vector2(1.0f, 1.0f)),
                //bottom
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f)), 
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f))
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
            Mesh mesh = new Mesh(vertices, indices);
            Model model = new Model(mesh);
            //model.AddTriangles(indices);
            //model.SetTexture(Renderer.Textures["default.png"]);
            return model;
        }

        /// <summary>
        /// Create a polygon model from an array of vertices. If the polygon is degenerate or non-simple then the model will be empty.
        /// </summary>
        public static Model CreatePolygon(IList<Vector2> vertices, Vector3 offset = new Vector3())
        {
            Mesh mesh = new Mesh();
            AddPolygon(mesh, vertices, offset);
            Model model = new Model(mesh);
            return model;
            //return CreatePolygon(PolygonFactory.CreatePolygon(vertices), offset);
        }

        public static Model CreatePolygon(Poly2Tri.Polygon polygon, Vector3 offset = new Vector3())
        {
            Mesh mesh = new Mesh();
            AddPolygon(mesh, polygon, offset);
            Model model = new Model(mesh);
            return model;
        }

        public static Model CreatePolygon(PolyTree polygon)
        {
            Mesh mesh = new Mesh();
            AddPolygon(mesh, polygon);
            Model model = new Model(mesh);
            return model;
        }

        public static int AddPolygon(Mesh model, IList<Vector2> v, Vector3 offset = new Vector3())
        {
            return AddPolygon(model, PolygonFactory.CreatePolygon(v), offset);
        }

        public static int AddPolygon(Mesh model, PolyTree polyTree, Vector3 offset = new Vector3())
        {
            return AddPolygon(model, PolygonFactory.CreatePolygon(polyTree).ToArray(), offset);
        }

        public static int AddPolygon(Mesh model, List<List<IntPoint>> paths, Vector3 offset = new Vector3())
        {
            return AddPolygon(model, PolygonFactory.CreatePolygon(paths).ToArray(), offset);
        }

        public static int AddPolygon(Mesh mesh, IList<Poly2Tri.Polygon> polygon, Vector3 offset = new Vector3())
        {
            int indexFirst = mesh.Vertices.Count;
            for (int i = 0; i < polygon.Count; i++)
            {
                int index = AddPolygon(mesh, polygon[i], new Vector3());
            }
            return indexFirst;
        }

        public static int AddPolygon(Mesh mesh, Poly2Tri.Polygon polygon, Vector3 offset = new Vector3())
        {
            int indexFirst = mesh.Vertices.Count;
            if (polygon == null)
            {
                return indexFirst;
            }
            int vertCountPrev = mesh.Vertices.Count;
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
                mesh.Vertices.Add(new Vertex(v, tc));
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
                mesh.AddTriangle(index0 + vertCountPrev, index1 + vertCountPrev, index2 + vertCountPrev);
            }
            return indexFirst;
        }

        public static Model CreateLines(Line[] lines)
        {
            Mesh mesh = new Mesh();
            for (int i = 0; i < lines.Length; i++)
            {
                Vector3 v;
                v = new Vector3(lines[i][0].X, lines[i][0].Y, 0);
                int index0 = mesh.AddVertex(new Vertex(v, new Vector2()));
                v = new Vector3(lines[i][1].X, lines[i][1].Y, 0);
                int index1 = mesh.AddVertex(new Vertex(v, new Vector2()));
                mesh.AddTriangle(index0, index1, index1);
            }
            Model model = new Model(mesh);
            model.Wireframe = true;
            return model;
        }

        public static Model CreateLinesWidth(Line[] lines, float width)
        {
            Mesh mesh = new Mesh();
            AddLinesWidth(mesh, lines, width);
            Model model = new Model(mesh);
            return model;
        }

        public static void AddLinesWidth(Mesh mesh, Line[] lines, float width)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 vStart, vEnd;
                vStart = lines[i][0];
                vEnd = lines[i][1];
                AddLineWidth(mesh, vStart, vEnd, width);
            }
        }

        public static Model CreateLineStripWidth(Vector2[] vertices, float width, bool closed)
        {
            Mesh mesh = new Mesh();
            AddLineStripWidth(mesh, vertices, width, closed);
            Model model = new Model(mesh);
            return model;
        }

        public static void AddLineStripWidth(Mesh mesh, Vector2[] vertices, float width, bool closed)
        {
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                AddLineWidth(mesh, vertices[i], vertices[i + 1], width);
            }
            if (closed)
            {
                AddLineWidth(mesh, vertices[vertices.Length - 1], vertices[0], width);
            }
        }

        private static void AddLineWidth(Mesh mesh, Vector2 v0, Vector2 v1, float width)
        {
            Vector2[] vectors = PolygonFactory.CreateLineWidth(new Line(v0, v1), width);
            Vertex[] vertices = new Vertex[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vertices[i] = new Vertex(vectors[i]);
            }
            int index = mesh.AddVertexRange(vertices);
            mesh.AddTriangle(index + 2, index + 1, index);
            mesh.AddTriangle(index, index + 3, index + 2);
        }

        public static Model CreateLineStrip(Vector2[] vertices, Vector3[] colors)
        {
            Debug.Assert(vertices.Length >= 2);
            Mesh mesh = new Mesh();
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                Vector3 v, color = new Vector3();
                if (colors != null)
                {
                    color = colors[i];
                }
                
                v = new Vector3(vertices[i].X, vertices[i].Y, 0);
                int index0 = mesh.AddVertex(new Vertex(v, new Vector2(), color));
                v = new Vector3(vertices[i + 1].X, vertices[i + 1].Y, 0);
                int index1 = mesh.AddVertex(new Vertex(v, new Vector2(), color));
                mesh.AddTriangle(index0, index1, index1);
            }
            Model model = new Model(mesh);
            model.Wireframe = true;
            return model;
        }

        public static Model CreateLineStrip(Vector2[] vertices)
        {
            return CreateLineStrip(vertices, null);
        }

        public static Model CreateCircle(Vector3 origin, float radius, int detail)
        {
            Debug.Assert(detail >= 3, "Detail must be greater or equal to 3.");
            Mesh mesh = new Mesh();
            for (int i = 0; i < detail; i++)
            {
                double rad = Math.PI * 2 * i / detail;
                Vector3 pos = new Vector3((float)Math.Cos(rad), (float)Math.Sin(rad), 0) * radius + origin;
                Vector2 textureCoord = new Vector2((float)(1 + Math.Cos(rad) / 2), (float)(1 + Math.Sin(rad) / 2));
                mesh.Vertices.Add(new Vertex(pos, textureCoord));
            }

            for (int i = 0; i < detail - 1; i++)
            {
                if (i == detail - 1 - i || i + 1 == detail - 1 - i)
                {
                    continue;
                }
                mesh.AddTriangle(i, i + 1, detail - 1 - i);
            }
            Model model = new Model(mesh);
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
            Mesh mesh = new Mesh();
            AddArrow(mesh, origin, pointAt, lineThickness, arrowLength, arrowThickness);
            return new Model(mesh);
        }

        public static void AddArrow(Mesh mesh, Vector3 origin, Vector2 pointAt, float lineThickness, float arrowLength, float arrowThickness)
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
            AddPolygon(mesh, polygon, origin);
        }
    }
}

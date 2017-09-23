using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClipperLib;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Game.Common;
using Game.Models;
using OpenTK;
using Poly2Tri;
using OpenTK.Graphics;

namespace Game.Rendering
{
    public static class ModelFactory
    {
        public static Model CreatePlane() => CreatePlane(new Vector2(1, 1), Color4.White);

        public static Model CreatePlane(Vector2 scale, Color4 color, Vector3 offset = new Vector3())
        {
            return new Model(CreatePlaneMesh(scale, color, offset));
        }

        public static Mesh CreatePlaneMesh(Vector2 scale, Color4 color, Vector3 offset = new Vector3())
        {
            Vertex[] vertices = {
                new Vertex(new Vector3(0f, scale.Y,  0f) + offset, new Vector2(0, 0), color),
                new Vertex(new Vector3(scale.X, scale.Y,  0f) + offset, new Vector2(1, 0), color),
                new Vertex(new Vector3(scale.X, 0f,  0f) + offset, new Vector2(1, 1), color),
                new Vertex(new Vector3(0f, 0f, 0f) + offset, new Vector2(0, 1), color)
            };

            var mesh = new Mesh { Vertices = vertices.ToList() };
            mesh.Indices.AddRange(new[] { 0, 2, 1 });
            mesh.Indices.AddRange(new[] { 0, 3, 2 });

            return mesh;
        }

        public static Mesh CreatePlaneMesh(Vector2 scale, Color4 c0, Color4 c1, Color4 c2, Color4 c3, Vector2 uvScale, Vector3 offset = new Vector3())
        {
            Vertex[] vertices = {
                new Vertex(new Vector3(0f, scale.Y,  0f) + offset, new Vector2(), c0),
                new Vertex(new Vector3(scale.X, scale.Y,  0f) + offset, uvScale.XOnly(), c1),
                new Vertex(new Vector3(scale.X, 0f,  0f) + offset, uvScale, c2),
                new Vertex(new Vector3(0f, 0f,  0f) + offset, uvScale.YOnly(), c3)
            };

            var mesh = new Mesh { Vertices = vertices.ToList() };
            mesh.Indices.AddRange(new[] { 0, 2, 1 });
            mesh.Indices.AddRange(new[] { 0, 3, 2 });

            return mesh;
        }

        public static Mesh CreatePlaneMesh(Vector2 topLeft, Vector2 bottomRight)
        {
            return CreatePlaneMesh(topLeft, bottomRight, Color4.White);
        }

        public static Mesh CreatePlaneMesh(Vector2 topLeft, Vector2 bottomRight, Color4 color)
        {
            return CreatePlaneMesh(topLeft, bottomRight, color, color, color, color);
        }

        public static Mesh CreatePlaneMesh(Vector2 topLeft, Vector2 bottomRight, Vector2 uvScale)
        {
            var color = Color4.White;
            return CreatePlaneMesh(bottomRight - topLeft, color, color, color, color, uvScale, new Vector3(topLeft));
        }

        public static Mesh CreatePlaneMesh(Vector2 topLeft, Vector2 bottomRight, Vector2 uvScale, Color4 color)
        {
            return CreatePlaneMesh(bottomRight - topLeft, color, color, color, color, uvScale, new Vector3(topLeft));
        }

        public static Mesh CreatePlaneMesh(
            Vector2 topLeft,
            Vector2 bottomRight,
            Color4 c0,
            Color4 c1,
            Color4 c2,
            Color4 c3)
        {
            return CreatePlaneMesh(bottomRight - topLeft, c0, c1, c2, c3, Vector2.One, new Vector3(topLeft));
        }

        public static Model CreateGrid(
            Vector2i gridSize,
            Vector2 gridTileSize,
            Color4 evenTileColor,
            Color4 oddTileColor,
            Vector3 offset = new Vector3())
        {
            Mesh mesh = new Mesh();
            for (int i = 0; i < gridSize.X; i++)
            {
                for (int j = 0; j < gridSize.Y; j++)
                {
                    var plane = CreatePlaneMesh(
                        gridTileSize,
                        (i + j) % 2 == 0 ? evenTileColor : oddTileColor,
                        new Vector3(i * gridTileSize.X, j * gridTileSize.Y, 0) + offset);
                    mesh.AddMesh(plane);
                }
            }
            return new Model(mesh);
        }

        public static Model CreateTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return CreateTriangle(v0, v1, v2, Color4.White);
        }

        public static Model CreateTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color4 color)
        {
            var mesh = new Mesh();
            mesh.AddTriangle(new Triangle(
                new Vertex(v0, new Vector2(), color),
                new Vertex(v1, new Vector2(), color),
                new Vertex(v2, new Vector2(), color)));
            return new Model(mesh);
        }

        public static Model CreateCube() => CreateCube(Color4.White);

        public static Model CreateCube(Vector3 scale) => CreateCube(scale, Color4.White);

        public static Model CreateCube(Color4 color) => CreateCube(new Vector3(1, 1, 1), color);

        public static Model CreateCube(Vector3 scale, Color4 color)
        {
            Vertex[] vertices = {
                //left
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f), color),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f) * scale, new Vector2(0.0f, 0.0f), color),
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f) * scale, new Vector2(0.0f, 1.0f), color),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f), color),
                //back
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f), color),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f), color),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f), color),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f), color),
                //right
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f), color),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f) * scale, new Vector2(1.0f, 1.0f), color),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f) * scale, new Vector2(1.0f, 0.0f), color),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f), color),
                //top
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f), color),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f), color),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f), color),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f), color),
                //front
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f) * scale, new Vector2(0.0f, 1.0f), color),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f) * scale, new Vector2(1.0f, 0.0f), color),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f) * scale, new Vector2(0.0f, 0.0f), color),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f) * scale, new Vector2(1.0f, 1.0f), color),
                //bottom
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 1.0f), color),
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f) * scale, new Vector2(1.0f, 0.0f), color),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 0.0f), color),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f) * scale, new Vector2(0.0f, 1.0f), color)
            };

            int[] indices = {
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
            var mesh = new Mesh(vertices, indices);
            var model = new Model(mesh);
            //model.AddTriangles(indices);
            //model.SetTexture(Renderer.Textures["default.png"]);
            return model;
        }

        public static Model CreatePolygon(IList<Vector2> vertices, Vector3 offset = new Vector3())
        {
            return CreatePolygon(vertices, Color4.White, offset);
        }

        /// <summary>
        /// Create a polygon model from an array of vertices. 
        /// If the polygon is degenerate or non-simple then the model will be empty.
        /// </summary>
        public static Model CreatePolygon(IList<Vector2> vertices, Color4 color, Vector3 offset = new Vector3())
        {
            var mesh = new Mesh();
            AddPolygon(mesh, vertices, color, offset);
            var model = new Model(mesh);
            return model;
            //return CreatePolygon(PolygonFactory.CreatePolygon(vertices), offset);
        }

        public static Model CreatePolygon(Polygon polygon, Color4 color, Vector3 offset = new Vector3())
        {
            var mesh = new Mesh();
            AddPolygon(mesh, polygon, color, offset);
            var model = new Model(mesh);
            return model;
        }

        public static Model CreatePolygon(PolyTree polygon, Color4 color)
        {
            var mesh = new Mesh();
            AddPolygon(mesh, polygon, color);
            var model = new Model(mesh);
            return model;
        }

        public static int AddPolygon(Mesh model, IList<Vector2> v, Color4 color, Vector3 offset = new Vector3())
        {
            return AddPolygon(model, PolygonFactory.CreatePolygon(v), color, offset);
        }

        public static int AddPolygon(Mesh model, PolyTree polyTree, Color4 color, Vector3 offset = new Vector3())
        {
            return AddPolygon(model, PolygonFactory.CreatePolygon(polyTree).ToArray(), color, offset);
        }

        public static int AddPolygon(Mesh model, List<List<IntPoint>> paths, Color4 color, Vector3 offset = new Vector3())
        {
            return AddPolygon(model, PolygonFactory.CreatePolygon(paths).ToArray(), color, offset);
        }

        public static int AddPolygon(Mesh mesh, IList<Poly2Tri.Polygon> polygon, Color4 color, Vector3 offset = new Vector3())
        {
            int indexFirst = mesh.Vertices.Count;
            for (int i = 0; i < polygon.Count; i++)
            {
                AddPolygon(mesh, polygon[i], color);
            }
            return indexFirst;
        }

        public static int AddPolygon(Mesh mesh, Poly2Tri.Polygon polygon, Color4 color, Vector3 offset = new Vector3())
        {
            int indexFirst = mesh.Vertices.Count;
            if (polygon == null)
            {
                return indexFirst;
            }
            int vertCountPrev = mesh.Vertices.Count;

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                TriangulationPoint p = polygon.Points[i];
                Vector3 v = new Vector3((float)p.X, (float)p.Y, 0) + offset;
                float tx = (float)((p.X - polygon.MinX) / (polygon.MaxX - polygon.MinX));
                float ty = (float)((p.Y - polygon.MinY) / (polygon.MaxY - polygon.MinY));
                var tc = new Vector2(tx, ty);
                //verts[i] = new Vertex(v, tc);
                mesh.Vertices.Add(new Vertex(v, tc, color));
            }

            foreach (DelaunayTriangle t in polygon.Triangles)
            {
                int index0 = polygon.IndexOf(t.Points._0);
                int index1 = polygon.IndexOf(t.Points._1);
                int index2 = polygon.IndexOf(t.Points._2);
                //Sometimes the index is -1 and I don't know why. Ignore those points.
                if (index0 < 0 || index1 < 0 || index2 < 0)
                {
                    continue;
                }
                mesh.AddTriangle(index0 + vertCountPrev, index1 + vertCountPrev, index2 + vertCountPrev);
            }
            return indexFirst;
        }

        public static Model CreateLines(IList<LineF> lines, Color4 color)
        {
            var mesh = new Mesh();
            AddLines(mesh, lines, color);
            var model = new Model(mesh) { Wireframe = true };
            return model;
        }

        public static void AddLines(Mesh mesh, IList<LineF> lines, Color4 color)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var v = new Vector3(lines[i][0].X, lines[i][0].Y, 0);
                int index0 = mesh.AddVertex(new Vertex(v, new Vector2(), color));
                v = new Vector3(lines[i][1].X, lines[i][1].Y, 0);
                int index1 = mesh.AddVertex(new Vertex(v, new Vector2(), color));
                mesh.AddTriangle(index0, index1, index1);
            }
        }

        public static Model CreateLinesWidth(LineF[] lines, float width, Color4 color)
        {
            var mesh = new Mesh();
            AddLinesWidth(mesh, lines, width, color);
            var model = new Model(mesh);
            return model;
        }

        public static void AddLinesWidth(Mesh mesh, LineF[] lines, float width, Color4 color)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                Vector2 vStart = lines[i][0];
                Vector2 vEnd = lines[i][1];
                AddLineWidth(mesh, vStart, vEnd, width, color);
            }
        }

        public static Model CreateLineStripWidth(Vector2[] vertices, float width, bool closed, Color4 color)
        {
            var mesh = new Mesh();
            AddLineStripWidth(mesh, vertices, width, closed, color);
            var model = new Model(mesh);
            return model;
        }

        public static void AddLineStripWidth(Mesh mesh, Vector2[] vertices, float width, bool closed, Color4 color)
        {
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                AddLineWidth(mesh, vertices[i], vertices[i + 1], width, color);
            }
            if (closed)
            {
                AddLineWidth(mesh, vertices[vertices.Length - 1], vertices[0], width, color);
            }
        }

        static void AddLineWidth(Mesh mesh, Vector2 v0, Vector2 v1, float width, Color4 color)
        {
            var vectors = PolygonFactory.CreateLineWidth(new LineF(v0, v1), width);
            var vertices = new Vertex[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                vertices[i] = new Vertex(new Vector3(vectors[i].X, vectors[i].Y, 0), new Vector2(), color);
            }
            int index = mesh.AddVertexRange(vertices);
            mesh.AddTriangle(index + 1, index + 2, index);
            mesh.AddTriangle(index, index + 2, index + 3);
        }

        public static Model CreateLineStrip(Vector2[] vertices, Color4[] colors)
        {
            DebugEx.Assert(vertices.Length >= 2);
            DebugEx.Assert(colors == null || vertices.Length == colors.Length);

            var mesh = new Mesh();
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                var color = new Color4();
                if (colors != null)
                {
                    color = colors[i];
                }

                var v = new Vector3(vertices[i].X, vertices[i].Y, 0);
                int index0 = mesh.AddVertex(new Vertex(v, new Vector2(), color));
                v = new Vector3(vertices[i + 1].X, vertices[i + 1].Y, 0);
                int index1 = mesh.AddVertex(new Vertex(v, new Vector2(), color));
                mesh.AddTriangle(index0, index1, index1);
            }
            var model = new Model(mesh) { Wireframe = true };
            return model;
        }

        public static Model CreateLineStrip(Vector2[] vertices) => CreateLineStrip(vertices, null);

        public static Model CreateCircle(Vector3 origin, float radius, int detail)
        {
            return CreateCircle(origin, radius, detail, Color4.White);
        }

        public static Model CreateCircle(Vector3 origin, float radius, int detail, Color4 color)
        {
            DebugEx.Assert(detail >= 3, "Detail must be greater or equal to 3.");
            var mesh = new Mesh();
            for (int i = 0; i < detail; i++)
            {
                double rad = Math.PI * 2 * i / detail;
                Vector3 pos = new Vector3((float)Math.Cos(rad), (float)Math.Sin(rad), 0) * radius + origin;
                var textureCoord = new Vector2((float)(1 + Math.Cos(rad) / 2), (float)(1 + Math.Sin(rad) / 2));
                mesh.Vertices.Add(new Vertex(pos, textureCoord, color));
            }

            for (int i = 0; i < detail - 1; i++)
            {
                if (i == detail - 1 - i || i + 1 == detail - 1 - i)
                {
                    continue;
                }
                mesh.AddTriangle(i, i + 1, detail - 1 - i);
            }
            return new Model(mesh);
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
        public static Model CreateArrow(Vector3 origin, Vector2 pointAt, float lineThickness, float arrowLength, float arrowThickness, Color4 color)
        {
            var mesh = new Mesh();
            AddArrow(mesh, origin, pointAt, lineThickness, arrowLength, arrowThickness, color);
            return new Model(mesh);
        }

        public static void AddArrow(Mesh mesh, Vector3 origin, Vector2 pointAt, float lineThickness, float arrowLength, float arrowThickness, Color4 color)
        {
            float length = pointAt.Length;
            Vector2[] polygon;
            if (length <= arrowLength)
            {
                polygon = new[] {
                    new Vector2(0, arrowLength),
                    new Vector2(-arrowThickness, 0),
                    new Vector2(arrowThickness, 0),
                };
            }
            else
            {
                polygon = new[] {
                    new Vector2(0, length),
                    new Vector2(-arrowThickness, length - arrowLength),
                    new Vector2(-lineThickness/2, length - arrowLength),
                    new Vector2(-lineThickness/2, 0),
                    new Vector2(lineThickness/2, 0),
                    new Vector2(lineThickness/2, length - arrowLength),
                    new Vector2(arrowThickness, length - arrowLength),
                };
            }
            polygon = Vector2Ex.Transform(polygon, Matrix4.CreateRotationZ((float)-(MathEx.VectorToAngleReversed(pointAt) + Math.PI / 2)));
            AddPolygon(mesh, polygon, color, origin);
        }
    }
}

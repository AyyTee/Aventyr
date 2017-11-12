using System.Collections.Generic;
using Game.Serialization;
using System.Linq;
using Game.Common;
using OpenTK;
using System;

namespace Game.Models
{
    public interface IMesh : IShallowClone<IMesh>
    {
        bool IsTransparent { get; }
        /// <summary>
        /// Get a shallow copy of list containing vertices in the mesh.
        /// </summary>
        List<Vertex> GetVertices();
        /// <summary>
        /// Get a shallow copy of list containing indices in the mesh.  Every 3 indices defines a triangle.
        /// </summary>
        List<int> GetIndices();
    }

    public static class IMeshEx
    {
        public static bool IsValid(this IMesh mesh)
        {
            var vertices = mesh.GetVertices();
            var indices = mesh.GetIndices();
            return indices.Count % 3 == 0 &&
                vertices.All(item => item != null) &&
                (indices.Count == 0 ||
                (indices.Max() < vertices.Count() && indices.Min() >= 0));
        }

        public static Mesh Combine(params IMesh[] meshes)
        {
            var meshNew = new Mesh();
            foreach (var mesh in meshes)
            {
                var offset = meshNew.Vertices.Count;
                var indices = mesh.GetIndices().Select(item => item + offset);
                meshNew.Indices.AddRange(indices);
                meshNew.Vertices.AddRange(mesh.GetVertices());
            }
            return meshNew;
        }

        public static IMesh Bisect(this IMesh mesh, LineF bisector, Side keepSide = Side.Left)
        {
            return Bisect(mesh, bisector, Matrix4.Identity, keepSide);
        }

        public static IMesh Bisect(this IMesh mesh, LineF bisector, Matrix4 transform, Side keepSide = Side.Left)
        {
            DebugEx.Assert(bisector != null);
            DebugEx.Assert(keepSide != Side.Neither);
            Triangle[] triangles = GetTriangles(mesh);

            var meshBisected = new Mesh();
            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle[] bisected = triangles[i].Transform(transform).Bisect(bisector, keepSide);
                meshBisected.AddTriangleRange(bisected);
            }
            meshBisected.RemoveDuplicates();
            meshBisected.Transform(transform.Inverted());
            return meshBisected;
        }

        public static IMesh Bisect(this IMesh mesh, ClipPath bisector)
        {
            return Bisect(mesh, bisector, Matrix4.Identity);
        }

        public static IMesh Bisect(this IMesh mesh, ClipPath bisector, Matrix4 transform)
        {
            DebugEx.Assert(MathEx.IsConvex(bisector.Path), "Only convex bisector supported for now.");
            var side = MathEx.IsClockwise(bisector.Path) ? Side.Right : Side.Left;

            IMesh meshNew = mesh;
            for (int i = 0; i < bisector.Path.Count; i++)
            {
                var iNext = (i + 1) % bisector.Path.Count;
                meshNew = Bisect(
                    meshNew,
                    new LineF(bisector.Path[i], bisector.Path[iNext]),
                    transform,
                    side);
            }
            return meshNew;
        }

        public static Vertex[] TriangleIntersections(Vertex[] vertices, Vector3 planeOrigin, Vector3 planeNormal)
        {
            DebugEx.Assert(vertices.Length == 3);

            var intersectT = new float[3];
            for (int i = 0; i < 3; i++)
            {
                var i1 = (i + 1) % 3;
                var lineOrigin = vertices[i].Position;
                var lineDelta = vertices[i1].Position - lineOrigin;
                var lineNormal = lineDelta.Normalized();
                var t = MathEx.LinePlaneIntersect(planeOrigin, planeNormal, lineOrigin, lineNormal) ?? -1;

                intersectT[i] = t / lineDelta.Length;
            }

            const float errorDelta = 0.0001f;

            var intersections = new Vertex[3];
            for (int i = 0; i < 3; i++)
            {
                var i1 = (i + 1) % 3;

                if (CornerCase(intersectT, i, errorDelta))
                {
                    intersections[i] = vertices[i1];
                    i++;
                }
                else if (MathEx.InsideRange(intersectT[i], 0, 1))
                {
                    intersections[i] = Vertex.Lerp(vertices[i], vertices[i1], intersectT[i]);
                }
            }

            //for (int i = 0; i < 3; i++)
            //{
            //    if (CornerCase(intersectT, i, errorDelta))
            //    {

            //    }
            //}

            var intersectionCount = intersections.Count(item => item != null);
            DebugEx.Assert(intersectionCount <= 2);

            return intersections;
        }

        /// <summary>
        /// Returns true if intersection is on the triangle corner.
        /// </summary>
        public static bool CornerCase(float[] intersectT, int index, float errorDelta)
        {
            return Math.Abs(intersectT[index] - 1) < errorDelta && Math.Abs(intersectT[(index + 1) % 3]) < errorDelta;
        }

        public static Mesh[] BisectPlane(this IMesh mesh, Vector3 planeOrigin, Vector3 planeNormal)
        {
            var vertices = mesh.GetVertices();
            var indices = mesh.GetIndices();

            var newIndices = new List<int>(indices.Count);

            for (int i = 0; i < indices.Count; i += 3)
            {

                //var intersections = TriangleIntersections(, planeOrigin, planeNormal);
                ////var Array.FindAll(intersections, item => item != null);
                //if (intersections == 2)
                //{

                //}

                //switch (intersections)
                //{
                //    case 0b110:
                //        vertices.Add(intersectVertices[1]);
                //        vertices.Add(intersectVertices[2]);
                        
                //        break;
                //    case 0b101:
                //        vertices.Add(intersectVertices[0]);
                //        vertices.Add(intersectVertices[2]);
                //        break;
                //    case 0b011:
                //        vertices.Add(intersectVertices[0]);
                //        vertices.Add(intersectVertices[1]);

                //        break;
                //    default:
                //        newIndices.Add(indices[i]);
                //        newIndices.Add(indices[i+1]);
                //        newIndices.Add(indices[i+2]);
                //        break;
                //}
            }
            
            return new[] { new Mesh(vertices, newIndices), null };
        }

        public static Triangle[] GetTriangles(this IMesh mesh)
        {
            List<Vertex> vertices = mesh.GetVertices();
            List<int> indices = mesh.GetIndices();
            Triangle[] triangles = new Triangle[indices.Count / 3];
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = new Triangle(
                    vertices[indices[i * 3]],
                    vertices[indices[i * 3 + 1]],
                    vertices[indices[i * 3 + 2]]);
            }
            return triangles;
        }
    }
}

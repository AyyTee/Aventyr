/*using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;

namespace Game
{
    public class ObjLoader
    {
        private string cfm(string str)
        {
            return str.Replace('.', ',');
        }

        public Model LoadStream(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            List<Vector3> points = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<Model.Triangle> tris = new List<Model.Triangle>();
            string line;
            char[] splitChars = { ' ' };
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(splitChars);
                line = line.Replace("  ", " ");

                string[] parameters = line.Split(splitChars);

                switch (parameters[0])
                {
                    case "p": // Point
                        break;

                    case "v": // Vertex
                        float x = float.Parse(cfm(parameters[1]));
                        float y = float.Parse(cfm(parameters[2]));
                        float z = float.Parse(cfm(parameters[3]));
                        points.Add(new Vector3(x, y, z));
                        break;

                    case "vt": // TexCoord
                        float u = float.Parse(cfm(parameters[1]));
                        float v = float.Parse(cfm(parameters[2]));
                        texCoords.Add(new Vector2(u, v));
                        break;

                    case "vn": // Normal
                        float nx = float.Parse(cfm(parameters[1]));
                        float ny = float.Parse(cfm(parameters[2]));
                        float nz = float.Parse(cfm(parameters[3]));
                        normals.Add(new Vector3(nx, ny, nz));
                        break;

                    case "f": // Face
                        tris.AddRange(parseFace(parameters));
                        break;
                }
            }

            Vector3[] p = points.ToArray();
            Vector2[] tc = texCoords.ToArray();
            Vector3[] n = normals.ToArray();
            Model.Triangle[] f = Model.Triangle.Vertices;

            return new Model(p, n, tc, f);
        }

        public MeshData LoadFile(string file)
        {
            // Silly me, using() closes the file automatically.
            using (FileStream s = File.Open(file, FileMode.Open))
            {
                return LoadStream(s);
            }
        }

        private static Model.Triangle[] parseFace(string[] indices)
        {
            Vertex[] p = new Vertex[indices.Length - 1];
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = parsePoint(indices[i + 1]);
            }
            return Triangulate(p);
            //return new Face(p);
        }

        // Takes an array of points and returns an array of triangles.
        // The points form an arbitrary polygon.
        private static Model.Triangle[] Triangulate(Vertex[] ps)
        {
            List<Model.Triangle> ts = new List<Model.Triangle>();
            if (ps.Length < 3)
            {
                throw new Exception("Invalid shape!  Must have >2 points");
            }

            Vertex lastButOne = ps[1];
            Vertex lastButTwo = ps[0];
            for (int i = 2; i < ps.Length; i++)
            {
                Model.Triangle t = new Model.Triangle(lastButTwo, lastButOne, ps[i]);
                lastButOne = ps[i];
                lastButTwo = ps[i - 1];
                ts.Add(t);
            }
            return ts.ToArray();
        }

        private static Vertex parsePoint(string s)
        {
            char[] splitChars = { '/' };
            string[] parameters = s.Split(splitChars);
            int vert = int.Parse(parameters[0]) - 1;
            int tex = int.Parse(parameters[1]) - 1;
            int norm = int.Parse(parameters[2]) - 1;
            return new Vertex(vert, norm, tex);
        }
    }
}*/
using System.Collections.Generic;
using System.IO;
using Game.Models;
using OpenTK;
using OpenTK.Graphics;

namespace Game.Rendering
{
    public class ModelLoader
    {
        const char SplitChar = ' ';
        /// <summary>
        /// Creates a Model using data from an obj file.  If unable to parse the file, null will be returned.
        /// </summary>
        public Model LoadObj(FileStream stream)
        {
            var reader = new StreamReader(stream);
            var vectorMap = new Dictionary<string, int>();
            var points = new List<Vector3>();
            var normals = new List<Vector3>();
            var texCoords = new List<Vector2>();
            Model model = new Model {Mesh = new Mesh()};
            string mtlFileName = "";
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(SplitChar);
                string[] parameters = line.Split(SplitChar);

                switch (parameters[0])
                {
                    case "mtllib":
                        mtlFileName = string.Join(SplitChar.ToString(), parameters, 1, parameters.Length - 1);
                        break;

                    case "p": // Point
                        break;

                    case "v": // Vertex
                        Vector3 vert;
                        if (!ParseVector3(parameters, out vert))
                        {
                            return null;
                        }
                        points.Add(vert);
                        break;

                    case "vt": // TexCoord
                        Vector2 tex;
                        if (!ParseVector2(parameters, out tex))
                        {
                            return null;
                        }
                        texCoords.Add(tex);
                        break;

                    case "vn": // Normal
                        Vector3 norm;
                        if (!ParseVector3(parameters, out norm))
                        {
                            return null;
                        }
                        normals.Add(norm);
                        break;

                    case "f": // Face
                        if (!ParseFace(model, parameters, points, normals, texCoords, vectorMap))
                        {
                            return null;
                        }
                        break;
                }
            }

            string mtlFilePath = Path.Combine(Path.GetDirectoryName(stream.Name), mtlFileName);
            model.SetTexture(LoadMtl(mtlFilePath));
            return model;
        }

        bool ParseFace(Model model, string[] parameters, List<Vector3> points, List<Vector3> normals, List<Vector2> texCoords, Dictionary<string, int> vectorMap)
        {
            string[] indices = parameters;
            int[] p = new int[indices.Length - 1];
            List<int> vertIndices = new List<int>();
            Mesh mesh = (Mesh)model.Mesh;
            for (int i = 0; i < p.Length; i++)
            {
                char[] splitCharsFace = { '/' };
                string[] subparameters = indices[i + 1].Split(splitCharsFace);
                int vertId, texId, normId;
                if (!ParseVertex(subparameters, out vertId, out texId, out normId))
                {
                    return false;
                }
                
                Vector2 texCoord = new Vector2();
                Vector3 normal = new Vector3();
                if (texId != -1)
                {
                    texCoord = texCoords[texId];
                }
                if (normId != -1)
                {
                    normal = normals[normId];
                }
                
                string key = GetKey(vertId, normId);
                if (vectorMap.ContainsKey(key))
                {
                    vertIndices.Add(vectorMap[key]);
                }
                else
                {
                    Vertex vertex = new Vertex(points[vertId], texCoord, new Color4(), normal);
                    int index = mesh.AddVertex(vertex);
                    vectorMap.Add(key, index);
                    vertIndices.Add(index);
                }
            }
            for (int i = 2; i < vertIndices.Count; i++)
            {
                mesh.Indices.Add(vertIndices[0]);
                mesh.Indices.Add(vertIndices[i - 1]);
                mesh.Indices.Add(vertIndices[i]);
            }
            return true;
        }

        bool ParseVertex(string[] parameters, out int vertId, out int texId, out int normId)
        {
            vertId = -1;
            texId = -1;
            normId = -1;
            if (parameters.Length < 3)
            {
                return false;
            }
            if (int.TryParse(parameters[0], out vertId))
            {
                vertId--;
            }
            else
            {
                return false;
            }
            if (int.TryParse(parameters[1], out texId))
            {
                texId--;
            }
            if (int.TryParse(parameters[2], out normId))
            {
                normId--;
            }
            return true;
        }

        static bool ParseVector3(string[] parameters, out Vector3 v)
        {
            if (parameters.Length != 4)
            {
                v = new Vector3();
                return false;
            }
            float x, y, z;
            bool valid = float.TryParse(parameters[1], out x);
            valid = float.TryParse(parameters[2], out y) && valid;
            valid = float.TryParse(parameters[3], out z) && valid;
            v = new Vector3(x, y, z);
            return valid;
        }

        bool ParseVector2(string[] parameters, out Vector2 v)
        {
            if (parameters.Length != 3)
            {
                v = new Vector2();
                return false;
            }
            float x, y;
            bool valid = float.TryParse(parameters[1], out x);
            valid = float.TryParse(parameters[2], out y) && valid;
            v = new Vector2(x, y);
            return valid;
        }

        public Model LoadObj(string file)
        {
            using (FileStream s = File.Open(file, FileMode.Open))
            {
                return LoadObj(s);
            }
        }

        public TextureFile LoadMtl(string file)
        {
            using (FileStream s = File.Open(file, FileMode.Open))
            {
                return LoadMtl(s);
            }
        }

        public TextureFile LoadMtl(FileStream stream)
        {
            var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(SplitChar);
                string[] parameters = line.Split(SplitChar);

                if (parameters[0] == "map_Kd")
                {
                    string textureFile = string.Join(SplitChar.ToString(), parameters, 1, parameters.Length - 1);
                    string textureFilePath = Path.Combine(Path.GetDirectoryName(stream.Name), textureFile);
                    return new TextureFile(textureFilePath);//Renderer.LoadImage(textureFilePath);
                }
            }
            return null;
        }

        static string GetKey(int verts, int norm)
        {
            return verts + " " + verts + " " + norm;
        }
    }
}
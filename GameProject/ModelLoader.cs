using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using System.Diagnostics;

namespace Game
{
    public class ModelLoader
    {
        const char splitChar = ' ';
        /// <summary>
        /// Creates a Model using data from an obj file.  If unable to parse the file, null will be returned.
        /// </summary>
        public Model LoadObj(FileStream stream)
        {
            StreamReader reader = new StreamReader(stream);
            Dictionary<string, int> vectorMap = new Dictionary<string, int>();
            List<Vector3> points = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            Model model = new Model();
            string mtlFileName = "";
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(splitChar);
                string[] parameters = line.Split(splitChar);

                switch (parameters[0])
                {
                    case "mtllib":
                        mtlFileName = string.Join(splitChar.ToString(), parameters, 1, parameters.Length - 1);
                        break;

                    case "p": // Point
                        break;

                    case "v": // Vertex
                        Vector3 vert;
                        if (!parseVector3(parameters, out vert))
                        {
                            return null;
                        }
                        points.Add(vert);
                        break;

                    case "vt": // TexCoord
                        Vector2 tex;
                        if (!parseVector2(parameters, out tex))
                        {
                            return null;
                        }
                        texCoords.Add(tex);
                        break;

                    case "vn": // Normal
                        Vector3 norm;
                        if (!parseVector3(parameters, out norm))
                        {
                            return null;
                        }
                        normals.Add(norm);
                        break;

                    case "f": // Face
                        if (!parseFace(model, parameters, points, normals, texCoords, vectorMap))
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

        private bool parseFace(Model model, string[] parameters, List<Vector3> points, List<Vector3> normals, List<Vector2> texCoords, Dictionary<string, int> vectorMap)
        {
            string[] indices = parameters;
            int[] p = new int[indices.Length - 1];
            List<int> vertIndices = new List<int>();
            for (int i = 0; i < p.Length; i++)
            {
                char[] splitCharsFace = { '/' };
                string[] subparameters = indices[i + 1].Split(splitCharsFace);
                int vertId, texId, normId;
                if (!parseVertex(subparameters, out vertId, out texId, out normId))
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
                
                string key = GetKey(vertId, texId, normId);
                if (vectorMap.ContainsKey(key))
                {
                    vertIndices.Add(vectorMap[key]);
                }
                else
                {
                    Vertex vertex = new Vertex(points[vertId], texCoord, new Vector3(), normal);
                    int index = 0;//model.AddVertex(vertex); //TODO
                    vectorMap.Add(key, index);
                    vertIndices.Add(index);
                }
            }
            for (int i = 2; i < vertIndices.Count; i++)
            {
                //model.AddTriangle(vertIndices[0], vertIndices[i - 1], vertIndices[i]); //TODO
            }
            return true;
        }

        private bool parseVertex(string[] parameters, out int vertId, out int texId, out int normId)
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

        private bool parseVector3(string[] parameters, out Vector3 v)
        {
            if (parameters.Length != 4)
            {
                v = new Vector3();
                return false;
            }
            float x = 0, y = 0, z = 0;
            bool valid = true;
            valid = valid && float.TryParse(parameters[1], out x);
            valid = valid && float.TryParse(parameters[2], out y);
            valid = valid && float.TryParse(parameters[3], out z);
            v = new Vector3(x, y, z);
            return valid;
        }

        private bool parseVector2(string[] parameters, out Vector2 v)
        {
            if (parameters.Length != 3)
            {
                v = new Vector2();
                return false;
            }
            float x = 0, y = 0;
            bool valid = true;
            valid = valid && float.TryParse(parameters[1], out x);
            valid = valid && float.TryParse(parameters[2], out y);
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
            StreamReader reader = new StreamReader(stream);
            string line;
            string textureFile = "";
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(splitChar);
                string[] parameters = line.Split(splitChar);

                if (parameters[0] == "map_Kd")
                {
                    textureFile = string.Join(splitChar.ToString(), parameters, 1, parameters.Length - 1);
                    string textureFilePath = Path.Combine(Path.GetDirectoryName(stream.Name), textureFile);
                    return new TextureFile(textureFilePath);//Renderer.LoadImage(textureFilePath);
                }
            }
            return null;
        }

        private string GetKey(int verts, int tex, int norm)
        {
            return verts.ToString() + " " + verts.ToString() + " " + norm.ToString();
        }
    }
}
using OpenTK;

namespace Game
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    public abstract class Model
    {
        public Transform Transform = new Transform();

        public int ibo_elements;
        public int VertCount;
        public int IndiceCount;
        public int ColorDataCount;
        public ShaderProgram Shader;

        public abstract Vector3[] GetVerts();
        public abstract int[] GetIndices(int offset = 0);
        public abstract Vector3[] GetColorData();

        public bool IsTextured = false;
        public int TextureID;
        public int TextureCoordsCount;
        public abstract Vector2[] GetTextureCoords();
    }
}

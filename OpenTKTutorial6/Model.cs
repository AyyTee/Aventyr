using OpenTK;

namespace Game
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    public abstract class Model
    {
        public Transform Transform = new Transform();
        public Vector3 Speed = Vector3.Zero;

        public int VertCount;
        public int IndiceCount;
        public int ColorDataCount;
        public Matrix4 VelocityMatrix = Matrix4.Identity;

        public abstract Vector3[] GetVerts();
        public abstract int[] GetIndices(int offset = 0);
        public abstract Vector3[] GetColorData();
        //public abstract void CalculateModelMatrix();

        public bool IsTextured = false;
        public int TextureID;
        public int TextureCoordsCount;
        public abstract Vector2[] GetTextureCoords();

    }
}

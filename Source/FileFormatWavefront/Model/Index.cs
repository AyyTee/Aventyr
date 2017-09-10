namespace FileFormatWavefront.Model
{
    /// <summary>
    /// Represents an index.
    /// </summary>
    public struct Index
    {
        /// <summary>
        /// The Vector3 index.
        /// </summary>
        public int Vertex;

        /// <summary>
        /// The uv index.
        /// </summary>
        public int? Uv;

        /// <summary>
        /// The normal index.
        /// </summary>
        public int? Normal;
    }
}
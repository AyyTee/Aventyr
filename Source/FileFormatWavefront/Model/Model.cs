using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FileFormatWavefront.Model
{
    /// <summary>
    /// Represent a scene of data loaded from an *.obj file.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Gets the vertices.
        /// </summary>
        public ImmutableArray<Vector3> Vertices { get; }
        /// <summary>
        /// Gets the uvs.
        /// </summary>
        public ImmutableArray<Vector2> Uvs { get; }
        /// <summary>
        /// Gets the normals.
        /// </summary>
        public ImmutableArray<Vector3> Normals { get; }
        /// <summary>
        /// Gets the faces which don't belong to any groups.
        /// </summary>
        public ImmutableArray<Face> UngroupedFaces { get; }
        /// <summary>
        /// Gets the groups.
        /// </summary>
        public ImmutableArray<Group> Groups { get; }
        /// <summary>
        /// Gets the materials.
        /// </summary>
        public ImmutableArray<Material> Materials { get; }
        /// <summary>
        /// Gets the name of the object in the file. This can be (and in many cases will be) null. 
        /// </summary>
        public string ObjectName { get; }

        internal Model(
            ImmutableArray<Vector3> vertices, 
            ImmutableArray<Vector2> uvs, 
            ImmutableArray<Vector3> normals, 
            ImmutableArray<Face> ungroupedFaces, 
            ImmutableArray<Group> groups, 
            ImmutableArray<Material> materials,
            string objectName)
        {
            Vertices = vertices;
            Uvs = uvs;
            Normals = normals;
            UngroupedFaces = ungroupedFaces;
            Groups = groups;
            Materials = materials;
            ObjectName = objectName;
        }
    }
}

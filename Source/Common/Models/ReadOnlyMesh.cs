using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Game.Common;
using System.Collections.Immutable;

namespace Game.Models
{
    public class ReadOnlyMesh : IMesh
    {
        public readonly ImmutableArray<Vertex> Vertices;
        public readonly ImmutableArray<int> Indices;

        public bool IsTransparent => Vertices.Any(item => item.Color.A < 1);

        public ReadOnlyMesh(IMesh mesh)
            : this(mesh.GetVertices().ToImmutableArray(), mesh.GetIndices().ToImmutableArray())
        {
        }

        public ReadOnlyMesh(ImmutableArray<Vertex> vertices, ImmutableArray<int> indices)
        {
            Vertices = vertices;
            Indices = indices;
            DebugEx.Assert(Indices.Length == 0 || Indices.Max() < Vertices.Length);
            DebugEx.Assert(Indices.Length % 3 == 0);
        }

        public List<Vertex> GetVertices()
        {
            return new List<Vertex>(Vertices);
        }

        public List<int> GetIndices()
        {
            return new List<int>(Indices);
        }

        public IMesh ShallowClone()
        {
            return new ReadOnlyMesh(Vertices, Indices);
        }
    }
}

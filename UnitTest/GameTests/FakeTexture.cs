using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;

namespace GameTests
{
    public class FakeTexture : ITexture
    {
        public Vector2i Size { get; }

        public bool IsTransparent { get; }

        public int Id { get; }

        public FakeTexture(ref int id, Vector2i size, bool isTransparent = true)
        {
            Id = id;
            id++;
            Size = size;
            IsTransparent = isTransparent;
        }
    }
}

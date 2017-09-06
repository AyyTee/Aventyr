using Game.Common;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class Glyph
    {
        public Vector2i Position { get; set; }
        public Vector2i Size => (Vector2i)Bitmap.Size;
        public Bitmap Bitmap { get; }

        public Glyph(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }
    }

    public class FontGlyph : Glyph
    {
        public FontFile Font { get; }
        public int CharIndex { get; }

        public FontGlyph(Bitmap bitmap, FontFile font, int charIndex)
            : base(bitmap)
        {
            Font = font;
            CharIndex = charIndex;
        }
    }

    public class TextureGlyph : Glyph
    {
        public string TexturePath { get; }

        public TextureGlyph(Bitmap bitmap, string texturePath)
            : base(bitmap)
        {
            TexturePath = texturePath;
        }
    }
}

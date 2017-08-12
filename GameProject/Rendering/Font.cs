using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Game.Models;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Game.Common;
using OpenTK.Graphics;
using MoreLinq;

namespace Game.Rendering
{
    public class Font
    {
        public class Settings
        {
            public Color4 Color { get; }
            public Vector2 Alignment { get; }
            public int LineSpacing { get; }
            public int CharSpacing { get; }

            public Settings(Vector2 alignment = new Vector2(), int lineSpacing = 0, int charSpacing = 0)
                : this(Color4.White, alignment, lineSpacing, charSpacing)
            {
            }

            public Settings(Color4 color, Vector2 alignment = new Vector2(), int lineSpacing = 0, int charSpacing = 0)
            {
                Color = color;
                Alignment = alignment;
                LineSpacing = lineSpacing;
                CharSpacing = charSpacing;
            }
        }

        readonly ITexture[] _fontTextures;
        readonly FontFile _fontFile;
        const int _indicesPerGlyph = 6;
        const int _verticesPerGlyph = 4;

        public Font(string fontDataFile)
        {
            _fontFile = FontLoader.Load(fontDataFile);
            
            _fontTextures = new TextureFile[_fontFile.Pages.Count];
            DebugEx.Assert(_fontTextures.Length == 1, "Multiple texture pages not supported yet.");
            var path = Path.GetDirectoryName(fontDataFile);
            for (int i = 0; i < _fontTextures.Length; i++)
            {
                _fontTextures[i] = new TextureFile(Path.Combine(path, _fontFile.Pages[i].File));
            }
        }

        public Font(FontFile fontFile, IEnumerable<ITexture> textures)
        {
            _fontFile = fontFile;
            _fontTextures = textures.ToArray();
        }

        public Vector2i GetSize(string text, Settings settings)
        {
            var lineHeight = _fontFile.Info.Size + settings.LineSpacing;

            var glyphs = GetGlyphs(text, settings);
            return new Vector2i(
                glyphs.SelectMany(item => item).MaxOrNull(item => item.EndPoint.X) ?? 0, 
                Math.Max(0, glyphs.Length * lineHeight - settings.LineSpacing));
        }

        GlyphData[][] GetGlyphs(string text, Settings settings)
        {
            Debug.Assert(!text.Contains('\r'));
            var lineBreakText = text.Split('\n');
            var glyphs = lineBreakText.Select(textLine => new GlyphData[textLine.Length]).ToArray();

            var posCurrent = new Vector2i();
            for (int i = 0; i < lineBreakText.Length; i++)
            {
                for (int j = 0; j < lineBreakText[i].Length; j++)
                {
                    // Get the data for this character. If it doesn't exist we use the question mark instead.
                    var fontChar = _fontFile.CharLookup.GetOrDefault(lineBreakText[i][j]) ?? _fontFile.CharLookup['?'];

                    glyphs[i][j] = new GlyphData(posCurrent, fontChar);

                    if (j + 1 < lineBreakText[i].Length)
                    {
                        posCurrent += new Vector2i(GetKerning(lineBreakText[i][j], lineBreakText[i][j + 1]), 0);
                    }

                    posCurrent += new Vector2i(fontChar.XAdvance + settings.CharSpacing, 0);
                }
                posCurrent = new Vector2i(0, posCurrent.Y + _fontFile.Info.Size + settings.LineSpacing);
            }
            return glyphs;
        }

        public Model GetModel(string text, Vector2 alignment = new Vector2(), int lineSpacing = 0, int charSpacing = 0)
        {
            return GetModel(text, Color4.White, new Vector2(), lineSpacing, charSpacing);
        }

        public Model GetModel(string text, Color4 color, Vector2 alignment = new Vector2(), int lineSpacing = 0, int charSpacing = 0)
        {
            return GetModel(text, new Settings(color, alignment, lineSpacing, charSpacing));
        }

        /// <summary>
        /// Creates a model to render a string with
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alignment">Percentage of offset to apply to the text model. 
        /// (0,0) is top-left aligned, (0.5,0.5) is centered, and (1,1) is bottom-right aligned.</param>
        /// <param name="charSpacing"></param>
        /// <returns></returns>
        public Model GetModel(string text, Settings settings)
        {
            Debug.Assert(!text.Contains('\r'));
            var glyphs = GetGlyphs(text, settings);

            if (settings.Alignment != new Vector2())
            {
                int yMax = glyphs.Last().Last().EndPoint.Y;

                int yOffset = (int)(-yMax * settings.Alignment.Y);
                foreach (var line in glyphs)
                {
                    int lineWidth = line.Last()?.EndPoint.X ?? 0;
                    int xOffset = (int)(-lineWidth * settings.Alignment.X);
                    foreach (var data in line)
                    {
                        data.Point += new Vector2i(xOffset, yOffset);
                    }
                }
            }

            var vertices = GetVertices(glyphs.SelectMany(item => item), settings.Color);
            var textMesh = new Mesh(
                vertices, 
                GetIndices(vertices.Count / _verticesPerGlyph).ToList());

            DebugEx.Assert(textMesh.IsValid());
            var textModel = new Model(textMesh)
            {
                Texture = _fontTextures[0],
            };
            return textModel;
        }

        List<Vertex> GetVertices(IEnumerable<GlyphData> glyphData, Color4 color)
        {
            var vertices = new List<Vertex>();
            foreach (var data in glyphData)
            {
                var fontChar = data.FontChar;

                int uvX = fontChar.X;
                int uvY = fontChar.Y;
                Vector2 uvSize = new Vector2(_fontFile.Common.ScaleW, _fontFile.Common.ScaleH);
                var uvTopLeft = Vector2.Divide(new Vector2(uvX, uvY), uvSize);
                var uvTopRight = Vector2.Divide(new Vector2(uvX + fontChar.Width, uvY), uvSize);
                var uvBottomLeft = Vector2.Divide(new Vector2(uvX, uvY + fontChar.Height), uvSize);
                var uvBottomRight = Vector2.Divide(new Vector2(uvX + fontChar.Width, uvY + fontChar.Height), uvSize);

                var endPoint = data.EndPoint;
                var startPoint = data.StartPoint;

                vertices.Add(new Vertex(new Vector3(endPoint.X, startPoint.Y, 0), uvTopRight, color));
                vertices.Add(new Vertex(new Vector3(endPoint.X, endPoint.Y, 0), uvBottomRight, color));
                vertices.Add(new Vertex(new Vector3(startPoint.X, endPoint.Y, 0), uvBottomLeft, color));
                vertices.Add(new Vertex(new Vector3(startPoint.X, startPoint.Y, 0), uvTopLeft, color));
            }

            return vertices;
        }

        int GetKerning(Char first, Char second)
        {
            var fontCharNext = _fontFile.CharLookup.GetOrDefault(second);
            if (fontCharNext == null)
            {
                return 0;
            }
            return _fontFile.KerningLookup[first].FirstOrDefault(item => item.Second == fontCharNext.ID)?.Amount ?? 0;
        }

        int[] GetIndices(int glyphCount)
        {
            var indices = new int[glyphCount * _indicesPerGlyph];
            for (int i = 0; i < glyphCount; i++)
            {
                int vertexIndex = i * _verticesPerGlyph;
                int indiceIndex = i * _indicesPerGlyph;
                indices[indiceIndex + 0] = vertexIndex;
                indices[indiceIndex + 1] = vertexIndex + 1;
                indices[indiceIndex + 2] = vertexIndex + 2;
                indices[indiceIndex + 3] = vertexIndex;
                indices[indiceIndex + 4] = vertexIndex + 2;
                indices[indiceIndex + 5] = vertexIndex + 3;
            }
            return indices;
        }

        class GlyphData
        {
            public Vector2i Point;
            public FontChar FontChar;

            public GlyphData(Vector2i point, FontChar fontChar)
            {
                Point = point;
                FontChar = fontChar;
            }

            public Vector2i StartPoint => new Vector2i(FontChar.XOffset, FontChar.YOffset) + Point;
            public Vector2i EndPoint => new Vector2i(FontChar.Width + FontChar.XOffset, FontChar.Height + FontChar.YOffset) + Point;
        }
    }
}

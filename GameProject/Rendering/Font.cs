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
            /// <summary>
            /// Horizontal text alignment. 
            /// Left aligned = 0, 
            /// Center aligned = 0.5, 
            /// Right aligned = 1
            /// </summary>
            public float AlignX { get; }
            public int LineSpacing { get; }
            public int CharSpacing { get; }

            public Settings(float alignX = 0, int lineSpacing = 0, int charSpacing = 0)
                : this(Color4.White, alignX, lineSpacing, charSpacing)
            {
            }

            public Settings(Color4 color, float alignX = 0, int lineSpacing = 0, int charSpacing = 0)
            {
                Color = color;
                AlignX = alignX;
                LineSpacing = lineSpacing;
                CharSpacing = charSpacing;
            }
        }

        readonly ITexture[] _fontTextures;
        public FontFile FontData { get; }
        const int _indicesPerGlyph = 6;
        const int _verticesPerGlyph = 4;

        public Font(string fontDataFile)
        {
            FontData = FontLoader.Load(fontDataFile);
            
            _fontTextures = new TextureFile[FontData.Pages.Count];
            DebugEx.Assert(_fontTextures.Length == 1, "Multiple texture pages not supported yet.");
            var path = Path.GetDirectoryName(fontDataFile);
            for (int i = 0; i < _fontTextures.Length; i++)
            {
                _fontTextures[i] = new TextureFile(Path.Combine(path, FontData.Pages[i].File));
            }
        }

        public Font(FontFile fontFile, IEnumerable<ITexture> textures)
        {
            FontData = fontFile;
            _fontTextures = textures.ToArray();
        }

        public Vector2i GetSize(string text, Settings settings)
        {
            var lineHeight = FontData.Common.LineHeight + settings.LineSpacing;

            var glyphs = GetGlyphs(text, settings);
            return new Vector2i(
                glyphs.SelectMany(item => item).MaxOrNull(item => item.EndPoint.X) ?? 0, 
                Math.Max(0, glyphs.Length * lineHeight - settings.LineSpacing));
        }

        public Vector2i BaselinePosition(string text, int charIndex, Settings settings)
        {
            Debug.Assert(text != null);

            var glyphs = GetGlyphs(text, settings);
            var lineNumber = 0;
            while (lineNumber < glyphs.Length && charIndex > glyphs[lineNumber].Length)
            {
                charIndex -= glyphs[lineNumber].Length + 1;
                lineNumber++;
            }

            var y = (FontData.Common.LineHeight + settings.LineSpacing) * lineNumber + FontData.Common.Base;
            if (glyphs[lineNumber].Length == 0)
            {
                return new Vector2i(0, y);
            }
            else if (charIndex == glyphs[lineNumber].Length)
            {
                var glyph = glyphs[lineNumber][charIndex - 1];
                return new Vector2i(glyph.Point.X + glyph.FontChar.XAdvance, y);
            }
            
            return new Vector2i(glyphs[lineNumber][charIndex].Point.X, y);
        }

        public Glyph[][] GetGlyphs(string text, Settings settings)
        {
            Debug.Assert(!text.Contains('\r'));
            var lineBreakText = text.Split('\n');
            var glyphs = lineBreakText.Select(textLine => new Glyph[textLine.Length]).ToArray();

            var posCurrent = new Vector2i();
            for (int i = 0; i < lineBreakText.Length; i++)
            {
                for (int j = 0; j < lineBreakText[i].Length; j++)
                {
                    // Get the data for this character. If it doesn't exist we use the question mark instead.
                    var fontChar = FontData.CharLookup.GetOrDefault(lineBreakText[i][j]) ?? FontData.CharLookup['?'];

                    glyphs[i][j] = new Glyph(posCurrent, fontChar);

                    if (j + 1 < lineBreakText[i].Length)
                    {
                        posCurrent += new Vector2i(GetKerning(lineBreakText[i][j], lineBreakText[i][j + 1]), 0);
                    }

                    posCurrent += new Vector2i(fontChar.XAdvance + settings.CharSpacing, 0);
                }
                posCurrent = new Vector2i(0, posCurrent.Y + FontData.Common.LineHeight + settings.LineSpacing);
            }
            return glyphs;
        }

        public Model GetModel(string text, float alignX = 0, int lineSpacing = 0, int charSpacing = 0)
        {
            return GetModel(text, Color4.White, 0, lineSpacing, charSpacing);
        }

        public Model GetModel(string text, Color4 color, float alignX = 0, int lineSpacing = 0, int charSpacing = 0)
        {
            return GetModel(text, new Settings(color, alignX, lineSpacing, charSpacing));
        }

        /// <summary>
        /// Creates a model to render a string with
        /// </summary>
        public Model GetModel(string text, Settings settings)
        {
            Debug.Assert(!text.Contains('\r'));
            var glyphs = GetGlyphs(text, settings);

            if (settings.AlignX != 0)
            {
                foreach (var line in glyphs)
                {
                    int lineWidth = line.Last()?.EndPoint.X ?? 0;
                    int xOffset = (int)(-lineWidth * settings.AlignX);
                    foreach (var data in line)
                    {
                        data.Point += new Vector2i(xOffset, 0);
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

        List<Vertex> GetVertices(IEnumerable<Glyph> glyphData, Color4 color)
        {
            var vertices = new List<Vertex>();
            foreach (var data in glyphData)
            {
                var fontChar = data.FontChar;

                int uvX = fontChar.X;
                int uvY = fontChar.Y;
                Vector2 uvSize = new Vector2(FontData.Common.ScaleW, FontData.Common.ScaleH);
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
            var fontCharNext = FontData.CharLookup.GetOrDefault(second);
            if (fontCharNext == null)
            {
                return 0;
            }
            return FontData.KerningLookup[first].FirstOrDefault(item => item.Second == fontCharNext.ID)?.Amount ?? 0;
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

        public class Glyph
        {
            public Vector2i Point;
            public FontChar FontChar;

            public Glyph(Vector2i point, FontChar fontChar)
            {
                Point = point;
                FontChar = fontChar;
            }

            public Vector2i StartPoint => new Vector2i(FontChar.XOffset, FontChar.YOffset) + Point;
            public Vector2i EndPoint => new Vector2i(FontChar.Width + FontChar.XOffset, FontChar.Height + FontChar.YOffset) + Point;
        }
    }
}

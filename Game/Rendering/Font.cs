using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Game.Models;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.IO;
using Game.Common;
using OpenTK.Graphics;
using MoreLinq;
using System.Runtime.Serialization;

namespace Game.Rendering
{
    [DataContract]
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
            public int? MaxWidth { get; }

            public Settings(float alignX = 0, int lineSpacing = 0, int charSpacing = 0, int? maxWidth = null)
                : this(Color4.White, alignX, lineSpacing, charSpacing, maxWidth)
            {
            }

            public Settings(Color4 color, float alignX = 0, int lineSpacing = 0, int charSpacing = 0, int? maxWidth = null)
            {
                DebugEx.Assert(maxWidth == null || maxWidth >= 0);
                Color = color;
                AlignX = alignX;
                LineSpacing = lineSpacing;
                CharSpacing = charSpacing;
                MaxWidth = maxWidth;
            }
        }

        [DataMember]
        public ITexture[] FontTextures { get; private set; }
        [DataMember]
        public FontFile FontData { get; private set; }
        const int _indicesPerGlyph = 6;
        const int _verticesPerGlyph = 4;

        public Font()
        {
        }

        public Font(FontFile fontFile, IEnumerable<ITexture> textures)
        {
            FontData = fontFile;
            FontTextures = textures.ToArray();
        }

        public Vector2i GetSize(string text)
        {
            return GetSize(text, new Settings());
        }

        public Vector2i GetSize(string text, Settings settings)
        {
            return _getSize(GetGlyphs(text, settings), settings);
        }

        Vector2i _getSize(Glyph[][] glyphs, Settings settings)
        {
            var lineHeight = FontData.Common.LineHeight + settings.LineSpacing;
            return new Vector2i(
                glyphs.SelectMany(item => item).MaxOrNull(item => item.EndPoint.X) ?? 0,
                Math.Max(0, lineHeight * glyphs.Length - settings.LineSpacing));
        }

        public Vector2i BaselinePosition(string text, int charIndex)
        {
            return BaselinePosition(text, charIndex, new Settings());
        }

        public Vector2i BaselinePosition(string text, int charIndex, Settings settings)
        {
            DebugEx.Assert(text != null);

            var index = MathHelper.Clamp(charIndex, 0, text.Length);

            var glyphs = GetGlyphs(text, settings);
            var lineNumber = 0;
            while (lineNumber < glyphs.Length && index > glyphs[lineNumber].Length)
            {
                index -= glyphs[lineNumber].Length + 1;
                lineNumber++;
            }

            var y = (FontData.Common.LineHeight + settings.LineSpacing) * lineNumber + FontData.Common.Base;
            if (glyphs[lineNumber].Length == 0)
            {
                return new Vector2i(0, y);
            }
            else if (index == glyphs[lineNumber].Length)
            {
                var glyph = glyphs[lineNumber][index - 1];
                return new Vector2i(glyph.Point.X + glyph.FontChar.XAdvance, y);
            }
            
            return new Vector2i(glyphs[lineNumber][index].Point.X, y);
        }

        public Glyph[][] GetGlyphs(string text, Settings settings)
        {
            DebugEx.Assert(!text.Contains('\r'));
            var lineBreakText = text.Replace("\t", "    ").Split('\n');
            var glyphs = new List<List<Glyph>>();

            var posCurrent = new Vector2i();
            for (int i = 0; i < lineBreakText.Length; i++)
            {
                glyphs.Add(new List<Glyph>());
                for (int j = 0; j < lineBreakText[i].Length; j++)
                {
                    var glyphLine = glyphs.Last();

                    // Get the data for this character. If it doesn't exist we use the question mark instead.
                    var fontChar = FontData.CharLookup.GetOrDefault(lineBreakText[i][j]) ?? FontData.CharLookup['?'];

                    glyphLine.Add(new Glyph(posCurrent, fontChar, lineBreakText[i][j]));

                    if (j + 1 < lineBreakText[i].Length)
                    {
                        posCurrent += new Vector2i(GetKerning(lineBreakText[i][j], lineBreakText[i][j + 1]), 0);
                    }

                    posCurrent += new Vector2i(fontChar.XAdvance + settings.CharSpacing, 0);

                    // Handle text wrapping.
                    if (settings.MaxWidth != null && glyphLine.Last().EndPoint.X > settings.MaxWidth)
                    {
                        if (glyphLine.Count > 1)
                        {
                            int nextIndex = WordStart(glyphLine, glyphLine.Count - 1);
                            if (nextIndex > 0)
                            {
                                j -= glyphLine.Count - nextIndex;
                                glyphLine.RemoveRange(nextIndex, glyphLine.Count - nextIndex);
                            }
                            else
                            {
                                // If the line has no split points then we just break off the last char and continue on the next line.
                                j--;
                                glyphLine.RemoveRange(glyphLine.Count - 1, 1);
                            }
                        }

                        if (j + 1 >= lineBreakText[i].Length)
                        {
                            break;
                        }

                        posCurrent = AdvanceLine(posCurrent, settings);
                        glyphs.Add(new List<Glyph>());
                    }
                }
                posCurrent = AdvanceLine(posCurrent, settings);
            }

            var glyphArray = glyphs.Select(item => item.ToArray()).ToArray();

            AlignGlyphs(glyphArray, settings);

            return glyphArray;
        }

        private void AlignGlyphs(Glyph[][] glyphs, Settings settings)
        {
            if (settings.AlignX != 0 && glyphs.Length > 1)
            {
                var maxWidth = _getSize(glyphs, settings).X;
                for (int i = 0; i < glyphs.Length; i++)
                {
                    if (glyphs[i].Length == 0)
                    {
                        continue;
                    }
                    int lineWidth = glyphs[i].Last().EndPoint.X;
                    int xOffset = (int)((maxWidth - lineWidth) * settings.AlignX);
                    for (int j = 0; j < glyphs[i].Length; j++)
                    {
                        glyphs[i][j].Point += new Vector2i(xOffset, 0);
                    }
                }
            }
        }

        private static int WordStart(List<Glyph> text, int charIndex)
        {
            while (charIndex > 0)
            {
                if (!char.IsWhiteSpace(text[charIndex].Char) &&
                    char.IsWhiteSpace(text[charIndex - 1].Char))
                {
                    break;
                }
                charIndex--;
            }
            return charIndex;
        }

        private Vector2i AdvanceLine(Vector2i posCurrent, Settings settings)
        {
            return new Vector2i(0, posCurrent.Y + FontData.Common.LineHeight + settings.LineSpacing);
        }

        public Model GetModel(string text, float alignX = 0, int lineSpacing = 0, int charSpacing = 0, int? maxWidth = null)
        {
            return GetModel(text, Color4.White, 0, lineSpacing, charSpacing, maxWidth);
        }

        public Model GetModel(string text, Color4 color, float alignX = 0, int lineSpacing = 0, int charSpacing = 0, int? maxWidth = null)
        {
            return GetModel(text, new Settings(color, alignX, lineSpacing, charSpacing, maxWidth));
        }

        /// <summary>
        /// Creates a model to render a string with
        /// </summary>
        public Model GetModel(string text, Settings settings)
        {
            DebugEx.Assert(!text.Contains('\r'));
            var glyphs = GetGlyphs(text, settings);

            var vertices = GetVertices(glyphs.SelectMany(item => item), settings.Color);
            var textMesh = new Mesh(
                vertices, 
                GetIndices(vertices.Count / _verticesPerGlyph).ToList());

            DebugEx.Assert(textMesh.IsValid());
            var textModel = new Model(textMesh)
            {
                Texture = FontTextures[0],
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
            return FontData.KerningLookup[first].FirstOrDefault(item => item.Second == fontCharNext.Id)?.Amount ?? 0;
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
            public Vector2i Point { get; set; }
            public FontChar FontChar { get; }
            public char Char { get; }

            public Vector2i StartPoint => new Vector2i(FontChar.XOffset, FontChar.YOffset) + Point;
            public Vector2i EndPoint => new Vector2i(FontChar.Width + FontChar.XOffset, FontChar.Height + FontChar.YOffset) + Point;

            public Glyph(Vector2i point, FontChar fontChar, char @char)
            {
                Point = point;
                FontChar = fontChar;
                Char = @char;
            }

            public override string ToString() => Char.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Game.Models;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace Game.Rendering
{
    public class FontRenderer
    {
        readonly TextureFile[] _fontTextures;
        readonly FontFile _fontFile;
        const int _indicesPerGlyph = 6;
        const int _verticesPerGlyph = 4;

        public FontRenderer(string fontDataFile)
        {
            _fontFile = FontLoader.Load(fontDataFile);

            _fontTextures = new TextureFile[_fontFile.Pages.Count];
            Debug.Assert(_fontTextures.Length == 1, "Multiple texture pages not supported yet.");
            var path = Path.GetDirectoryName(fontDataFile);
            for (int i = 0; i < _fontTextures.Length; i++)
            {
                _fontTextures[i] = new TextureFile(Path.Combine(path, _fontFile.Pages[i].File));
            }
        }

        /// <summary>
        /// Creates a model to render a string with
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Model GetModel(string text)
        {
            return GetModel(text, new Vector2(0, 0), 0);
        }

        /// <summary>
        /// Creates a model to render a string with
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alignment">Percentage of offset to apply to the text model. 
        /// (0,0) is top-left aligned, (0.5,0.5) is centered, and (1,1) is bottom-right aligned.</param>
        /// <param name="charSpacing"></param>
        /// <returns></returns>
        public Model GetModel(string text, Vector2 alignment, int charSpacing)
        {
            var textMesh = new Mesh();

            //var lineBreakText = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            //var vertices = new Vertex[lineBreakText.Sum(textLine => textLine.Length) * _verticesPerGlyph];
            var vertices = new Vertex[text.Length * _verticesPerGlyph];
            int x0 = 0;
            int y0 = 0;

            //var glyphPositions = lineBreakText.Select(textLine => new Vector2[textLine.Length],);

            for (int i = 0; i < text.Length; i++)
            {
                // Get the data for this character. If it doesn't exist we use the question mark instead.
                var fontChar = _fontFile.CharLookup.GetOrDefault(text[i]) ?? _fontFile.CharLookup['?'];

                int index = i * _verticesPerGlyph;

                int uvX = fontChar.X;
                int uvY = fontChar.Y;
                Vector2 uvSize = new Vector2(_fontFile.Common.ScaleW, _fontFile.Common.ScaleH);
                var uvTopLeft = Vector2.Divide(new Vector2(uvX, uvY), uvSize);
                var uvTopRight = Vector2.Divide(new Vector2(uvX + fontChar.Width, uvY), uvSize);
                var uvBottomLeft = Vector2.Divide(new Vector2(uvX, uvY + fontChar.Height), uvSize);
                var uvBottomRight = Vector2.Divide(new Vector2(uvX + fontChar.Width, uvY + fontChar.Height), uvSize);

                var x2 = x0 + fontChar.XOffset;
                var x3 = x0 + fontChar.Width + fontChar.XOffset;
                var y2 = y0 - fontChar.YOffset;
                var y3 = y0 - fontChar.Height - fontChar.YOffset;
                
                vertices[index + 0] = new Vertex(new Vector3(x3, y2, 0), uvTopRight);
                vertices[index + 1] = new Vertex(new Vector3(x3, y3, 0), uvBottomRight);
                vertices[index + 2] = new Vertex(new Vector3(x2, y3, 0), uvBottomLeft);
                vertices[index + 3] = new Vertex(new Vector3(x2, y2, 0), uvTopLeft);

                if (i + 1 < text.Length)
                {
                    x0 += GetKerning(text[i], text[i + 1]);
                }

                x0 += fontChar.XAdvance + charSpacing;
            }
            //var offset = new Vector3((float)Math.Round(-x0 * alignment.X), (float)Math.Round(_charHeight * (1 - alignment.Y)), 0);
            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    Vector3 pos = vertices[i].Position + offset;
            //    vertices[i] = new Vertex(pos, vertices[i].TextureCoord);
            //}
            textMesh.Vertices = vertices.ToList();
            textMesh.Indices = AddIndices(text.Length).ToList();

            Debug.Assert(textMesh.IsValid());
            var textModel = new Model(textMesh)
            {
                Texture = _fontTextures[0],
                IsTransparent = true
            };
            return textModel;
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

        int[] AddIndices(int glyphCount)
        {
            var indices = new int[glyphCount * _indicesPerGlyph];
            for (int i = 0; i < glyphCount; i++)
            {
                int vertexIndex = i * _verticesPerGlyph;
                int indiceIndex = i * _indicesPerGlyph;
                indices[indiceIndex] = vertexIndex;
                indices[indiceIndex + 1] = vertexIndex + 1;
                indices[indiceIndex + 2] = vertexIndex + 2;
                indices[indiceIndex + 3] = vertexIndex;
                indices[indiceIndex + 4] = vertexIndex + 2;
                indices[indiceIndex + 5] = vertexIndex + 3;
            }
            return indices;
        }
    }
}

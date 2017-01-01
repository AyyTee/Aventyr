using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Game.Models;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Game.Rendering
{
    public class FontRenderer
    {
        readonly int _charHeight;
        OpenTK.Graphics.OpenGL.PixelFormat _format = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;

        public class CharData
        {
            public CharData(Rectangle pixelRegion, FontRenderer fontRenderer)
            {
                _pixelRegion = pixelRegion;
                _fontRenderer = fontRenderer;
            }
            Rectangle _pixelRegion;
            readonly FontRenderer _fontRenderer;

            public Rectangle PixelRegion => _pixelRegion;

            public Box2 UvRegion
            {
                get 
                {
                    var v0 = new Vector2(_pixelRegion.Left / (float)_fontRenderer._textureSize.Width, _pixelRegion.Bottom / (float)_fontRenderer._textureSize.Height);
                    var v1 = new Vector2(_pixelRegion.Right / (float)_fontRenderer._textureSize.Width, _pixelRegion.Top / (float)_fontRenderer._textureSize.Height);
                    return new Box2(v0, v1);
                }
            }
        }

        Size _textureSize = new Size(1024, 1024);
        public Texture Texture;
        readonly CharData[] _chars = new CharData[255];
        public FontRenderer(Font font)
        {
            Font font1 = font;
            _charHeight = -font1.Height;
            var glyphBitmap = new Bitmap(_textureSize.Width, _textureSize.Height);
            
            Texture = new Texture(GL.GenTexture());
            GL.BindTexture(TextureTarget.Texture2D, Texture.GetId());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            
            using (Graphics gfx = Graphics.FromImage(glyphBitmap))
            {
                var format = new StringFormat(StringFormat.GenericTypographic);
                var charPoint = new Point(0, 0);
                for (int i = 0; i < _chars.Length; i++)
                {
                    SizeF charSizeF = gfx.MeasureString(new string(new[] { Convert.ToChar(i) }), font1, 0, format);

                    var charSize = new Size((int)Math.Ceiling(charSizeF.Width), (int)Math.Ceiling(charSizeF.Height));

                    //fudge factor to prevent glyphs from overlapping
                    charSize.Width += 2;

                    if (charSize.Width + charPoint.X > _textureSize.Width)
                    {
                        charPoint.X = 0;
                        charPoint.Y += (int)Math.Ceiling(font1.GetHeight());
                    }
                    _chars[i] = new CharData(new Rectangle(charPoint, charSize), this);
                    charPoint.X += charSize.Width;
                }

                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                gfx.Clear(Color.Transparent);
                for (int i = 0; i < _chars.Length; i++)
                {
                    var point = new PointF(_chars[i].PixelRegion.X, _chars[i].PixelRegion.Y);
                    gfx.DrawString(new string(new[] { Convert.ToChar(i) }), font1, new SolidBrush(Color.Black), point);
                }
            }

            BitmapData data = glyphBitmap.LockBits(new Rectangle(0, 0, glyphBitmap.Width, glyphBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _textureSize.Width, _textureSize.Height, 0, _format, PixelType.UnsignedByte, data.Scan0);
            glyphBitmap.UnlockBits(data);
        }

        public CharData[] GetChar(int[] index)
        {
            CharData[] charData = new CharData[index.Length];
            for (int i = 0; i < index.Length; i++)
            {
                charData[i] = _chars[index[i]];
            }
            return charData;
        }

        public CharData[] GetChar(string index)
        {
            char[] charArray = index.ToCharArray();
            var charList = new List<char>(charArray);
            var intList = charList.ConvertAll(Convert.ToInt32);
            return GetChar(intList.ToArray());
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
        public Model GetModel(string text, Vector2 alignment, float charSpacing)
        {
            var textMesh = new Mesh();
            
            CharData[] charData = GetChar(text);
            var vertices = new Vertex[charData.Length * 4];
            var indices = new List<int>();
            float x0 = 0;
            for (int i = 0; i < charData.Length; i++)
            {
                int index = i * 4;
                float x1 = x0 + charData[i].PixelRegion.Width;
                vertices[index + 3] = new Vertex(new Vector3(x0, 0, 0), new Vector2(charData[i].UvRegion.Left, charData[i].UvRegion.Top));
                vertices[index + 2] = new Vertex(new Vector3(x0, charData[i].PixelRegion.Height, 0), new Vector2(charData[i].UvRegion.Left, charData[i].UvRegion.Bottom));
                vertices[index + 1] = new Vertex(new Vector3(x1, charData[i].PixelRegion.Height, 0), new Vector2(charData[i].UvRegion.Right, charData[i].UvRegion.Bottom));
                vertices[index] = new Vertex(new Vector3(x1, 0, 0), new Vector2(charData[i].UvRegion.Right, charData[i].UvRegion.Top));
                indices.AddRange(new[] { index, index + 1, index + 2, index, index + 2, index + 3 });
                x0 = x1 + charSpacing;
            }
            var offset = new Vector3((float)Math.Round(-x0 * alignment.X), (float)Math.Round(_charHeight * (1 - alignment.Y)), 0);
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 pos = vertices[i].Position + offset;
                vertices[i] = new Vertex(pos, vertices[i].TextureCoord);
            }
            textMesh.Vertices.AddRange(vertices);
            //textModel.Indices.AddRange(indices);
            //textMesh.AddTriangles(indices.ToArray());
            textMesh.Indices.AddRange(indices);

            var textModel = new Model(textMesh)
            {
                Texture = Texture,
                IsTransparent = true
            };
            return textModel;
        }
    }
}

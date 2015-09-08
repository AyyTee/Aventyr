using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Game
{
    public class FontRenderer
    {
        Bitmap GlyphBitmap;
        Font Font;
        OpenTK.Graphics.OpenGL.PixelFormat Format = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;

        public class CharData
        {
            public CharData(Rectangle pixelRegion, FontRenderer fontRenderer)
            {
                _pixelRegion = pixelRegion;
                _fontRenderer = fontRenderer;
            }
            private Rectangle _pixelRegion;
            private FontRenderer _fontRenderer;

            public Rectangle PixelRegion
            {
                get { return _pixelRegion; }
            }

            public Box2 UVRegion
            {
                get 
                {
                    Vector2 v0 = new Vector2(_pixelRegion.Left / (float)_fontRenderer.TextureSize.Width, _pixelRegion.Bottom / (float)_fontRenderer.TextureSize.Height);
                    Vector2 v1 = new Vector2(_pixelRegion.Right / (float)_fontRenderer.TextureSize.Width, _pixelRegion.Top / (float)_fontRenderer.TextureSize.Height);
                    return new Box2(v0, v1);
                }
            }
        }

        Size TextureSize = new Size(1024, 1024);
        public int textureID;
        CharData[] chars = new CharData[255];
        public FontRenderer(Font font)
        {
            Font = font;
            GlyphBitmap = new Bitmap(TextureSize.Width, TextureSize.Height);
            
            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            
            using (Graphics gfx = Graphics.FromImage(GlyphBitmap))
            {
                Point charPoint = new Point(0, 0);
                for (int i = 0; i < chars.Length; i++)
                {
                    SizeF charSizeF = gfx.MeasureString(new string(new Char[1] { Convert.ToChar(i) }), Font);
                    Size charSize = new Size((int)Math.Ceiling(charSizeF.Width), (int)Math.Ceiling(charSizeF.Height));
                    if (charSize.Width + charPoint.X > TextureSize.Width)
                    {
                        charPoint.X = 0;
                        charPoint.Y += (int)Math.Ceiling(Font.GetHeight());
                    }
                    chars[i] = new CharData(new Rectangle(charPoint, charSize), this);
                    charPoint.X += charSize.Width;
                }

                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                gfx.Clear(Color.Transparent);
                for (int i = 0; i < chars.Length; i++)
                {
                    PointF point = new PointF(chars[i].PixelRegion.X, chars[i].PixelRegion.Y);
                    gfx.DrawString(new string(new Char[1] { Convert.ToChar(i) }), Font, new SolidBrush(Color.Black), point);
                }
            }

            BitmapData data = GlyphBitmap.LockBits(new Rectangle(0, 0, GlyphBitmap.Width, GlyphBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureSize.Width, TextureSize.Height, 0, Format, PixelType.UnsignedByte, data.Scan0);
            GlyphBitmap.UnlockBits(data);
        }

        public CharData[] GetChar(int[] index)
        {
            CharData[] charData = new CharData[index.Length];
            for (int i = 0; i < index.Length; i++)
            {
                charData[i] = chars[index[i]];
            }
            return charData;
        }

        public CharData[] GetChar(string index)
        {
            char[] charArray = index.ToCharArray();
            var charList = new List<char>(charArray);
            List<int> intList = charList.ConvertAll(c => Convert.ToInt32(c));
            return GetChar(intList.ToArray());
        }

        /// <summary>
        /// Creates a model to render a string with
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alignment">Percentage of offset to apply to the text model. 
        /// (0,0) is top-left aligned, (0.5,0.5) is centered, and (1,1) is bottom-right aligned.</param>
        /// <returns></returns>
        public Model GetModel(String text, Vector2 alignment, float charSpacing)
        {
            Model textModel = new Model(Controller.Shaders["text"]);
            textModel.TextureID = textureID;
            CharData[] charData = GetChar(text);
            Model.Vertex[] vertices = new Model.Vertex[charData.Length * 4];
            List<int> indices = new List<int>();
            float x0 = 0, x1;
            for (int i = 0; i < charData.Length; i++)
            {
                int index = i * 4;
                x1 = x0 + charData[i].PixelRegion.Width;
                vertices[index] = new Model.Vertex(new Vector3(x0, 0, 0), new Vector2(charData[i].UVRegion.Left, charData[i].UVRegion.Top));
                vertices[index + 1] = new Model.Vertex(new Vector3(x0, charData[i].PixelRegion.Height, 0), new Vector2(charData[i].UVRegion.Left, charData[i].UVRegion.Bottom));
                vertices[index + 2] = new Model.Vertex(new Vector3(x1, charData[i].PixelRegion.Height, 0), new Vector2(charData[i].UVRegion.Right, charData[i].UVRegion.Bottom));
                vertices[index + 3] = new Model.Vertex(new Vector3(x1, 0, 0), new Vector2(charData[i].UVRegion.Right, charData[i].UVRegion.Top));
                indices.AddRange(new int[] { index, index + 1, index + 2, index, index + 2, index + 3 });
                x0 = x1 + charSpacing;
            }
            Vector3 offset = new Vector3((float)Math.Round(-x0 * alignment.X), (float)Math.Round(-Font.Height * (1 - alignment.Y)), 0);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position += offset;
            }
            textModel.Vertices.AddRange(vertices);
            textModel.Indices.AddRange(indices);
            return textModel;
        }
    }
}

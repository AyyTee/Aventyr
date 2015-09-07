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
        Point Size;
        Font Font;
        OpenTK.Graphics.OpenGL.PixelFormat Format = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;
        
        public int textureID;
        int charPerRow = 16;
        Point charSize = new Point(50, 1);
        int charCount = 255;
        public FontRenderer(Font font)
        {
            Font = font;
            //System.Drawing.Graphics.MeasureString();
            charSize.Y = (int)Font.GetHeight();
            Size = GetTextureSize();
            GlyphBitmap = new Bitmap(Size.X, Size.Y);
            
            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            
            using (Graphics gfx = Graphics.FromImage(GlyphBitmap))
            {
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                gfx.Clear(Color.Transparent);
                for (int i = 0; i < charCount; i++)
                {
                    Point p = GetCharCoord(i);
                    //p.Y += charSize.Y * 2;
                    gfx.DrawString(new string(new Char[1] { Convert.ToChar(i) }), Font, new SolidBrush(Color.Black), p);
                }
            }

            BitmapData data = GlyphBitmap.LockBits(new Rectangle(0, 0, GlyphBitmap.Width, GlyphBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Size.X, Size.Y, 0, Format, PixelType.UnsignedByte, data.Scan0);
            GlyphBitmap.UnlockBits(data);
        }

        public Point GetTextureSize()
        {
            int w = charPerRow * charSize.X;
            int h = (int)(charCount / charPerRow) * charSize.Y;
            w = 1 << (int)Math.Ceiling(Math.Log(w, 2));
            h = 1 << (int)Math.Ceiling(Math.Log(h, 2));
            return new Point(w, h);
        }

        public Point GetCharCoord(int index)
        {
            Debug.Assert(index < charCount);
            return new Point((index % charPerRow) * charSize.X, (int)Math.Floor((double)index / charPerRow) * charSize.Y);
        }

        public Box2 GetCharUV(int index)
        {
            Point p = GetCharCoord(index);
            Vector2 v0 = new Vector2(p.X / (float)Size.X, (p.Y + charSize.Y) / (float)Size.Y);
            Vector2 v1 = new Vector2((p.X + charSize.X) / (float)Size.X, p.Y / (float)Size.Y);
            return new Box2(v0, v1); //new Box2(new Vector2(), new Vector2(1, 1));//
        }

        public Box2[] GetCharUV(int[] index)
        {
            Box2[] uv = new Box2[index.Length];
            for (int i = 0; i < index.Length; i++)
            {
                uv[i] = GetCharUV(index[i]);
            }
            return uv;
        }

        public Box2[] GetCharUV(string index)
        {
            char[] charArray = index.ToCharArray();
            var charList = new List<char>(charArray);
            List<int> intList = charList.ConvertAll(c => Convert.ToInt32(c));
            return GetCharUV(intList.ToArray());
        }

        /// <summary>
        /// Creates a model to render a string with
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Model GetModel(String text)
        {
            float charSpacing = 1.1f;
            Model textModel = new Model(Controller.Shaders["text"]);
            textModel.TextureID = textureID;
            Box2[] uv = GetCharUV(text);
            Model.Vertex[] vertices = new Model.Vertex[uv.Length * 4];
            List<int> indices = new List<int>();
            for (int i = 0; i < uv.Length; i++)
            {
                float charWidth = Size.X / (float)Size.Y;
                int index = i * 4;
                float x0 = i * charWidth * charSpacing;
                float x1 = x0 + charWidth;
                vertices[index] = new Model.Vertex(new Vector3(x0, 0, 0), new Vector2(uv[i].Left, uv[i].Top));
                vertices[index + 1] = new Model.Vertex(new Vector3(x0, 1, 0), new Vector2(uv[i].Left, uv[i].Bottom));
                vertices[index + 2] = new Model.Vertex(new Vector3(x1, 1, 0), new Vector2(uv[i].Right, uv[i].Bottom));
                vertices[index + 3] = new Model.Vertex(new Vector3(x1, 0, 0), new Vector2(uv[i].Right, uv[i].Top));
                indices.AddRange(new int[] { index, index + 1, index + 2, index, index + 2, index + 3 });
            }
            textModel.Vertices.AddRange(vertices);
            textModel.Indices.AddRange(indices);
            return textModel;
        }
    }
}

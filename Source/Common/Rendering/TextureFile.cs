using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using OpenTK.Graphics.OpenGL;
using Game.Common;
using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Game.Rendering
{
    [DataContract]
    public class TextureFile : ITexture
    {
        Texture _texture;
        [DataMember]
        public string Filepath { get; }

        public bool IsTransparent => _texture.IsTransparent;
        public Vector2i Size => _texture.Size;
        public int Id => _texture.Id;

        public Common.RectangleF UvBounds => _texture.UvBounds;

        public TextureFile(string filepath, bool deferLoad = false)
        {
            Filepath = filepath;
            if (!deferLoad)
            {
                LoadImage();
            }
        }

        void LoadImage(Bitmap image)
        {
            if (_texture != null)
            {
                return;
            }

            int texId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texId);
            BitmapData data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(
                TextureTarget.Texture2D, 
                0, 
                PixelInternalFormat.Rgba, 
                data.Width, 
                data.Height, 
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, 
                PixelType.UnsignedByte, 
                data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            _texture = new Texture(texId, (Vector2i)image.Size, HasTransparency(image));
        }

        public void LoadImage()
        {
            if (_texture != null)
            {
                return;
            }

            try
            {
                using (var file = new Bitmap(Path.Combine(Resources.ResourcePath, Filepath)))
                {
                    LoadImage(file);
                }
            }
            catch (FileNotFoundException)
            {
                DebugEx.Assert(false, "Texture missing.");
            }
        }

        /// <summary>
        /// Determines if a bitmap image has any transparent pixels on it.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        /// <remarks>Original code found here: https://stackoverflow.com/a/39013496 </remarks>
        public static bool HasTransparency(Bitmap bitmap)
        {
            // not an alpha-capable color format.
            if ((bitmap.Flags & (Int32)ImageFlags.HasAlpha) == 0)
                return false;
            // Indexed formats. Special case because one index on their palette is configured as THE transparent color.
            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed || 
                bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format4bppIndexed)
            {
                ColorPalette pal = bitmap.Palette;
                // Find the transparent index on the palette.
                Int32 transCol = -1;
                for (int i = 0; i < pal.Entries.Length; i++)
                {
                    Color col = pal.Entries[i];
                    if (col.A != 255)
                    {
                        // Color palettes should only have one index acting as transparency. Not sure if there's a better way of getting it...
                        transCol = i;
                        break;
                    }
                }
                // none of the entries in the palette have transparency information.
                if (transCol == -1)
                    return false;
                // Check pixels for existence of the transparent index.
                Int32 colDepth = Image.GetPixelFormatSize(bitmap.PixelFormat);
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                Int32 stride = data.Stride;
                Byte[] bytes = new Byte[bitmap.Height * stride];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                bitmap.UnlockBits(data);
                if (colDepth == 8)
                {
                    // Last line index.
                    Int32 lineMax = bitmap.Width - 1;
                    for (Int32 i = 0; i < bytes.Length; i++)
                    {
                        // Last position to process.
                        Int32 linepos = i % stride;
                        // Passed last image byte of the line. Abort and go on with loop.
                        if (linepos > lineMax)
                            continue;
                        Byte b = bytes[i];
                        if (b == transCol)
                            return true;
                    }
                }
                else if (colDepth == 4)
                {
                    // line size in bytes. 1-indexed for the moment.
                    Int32 lineMax = bitmap.Width / 2;
                    // Check if end of line ends on half a byte.
                    bool halfByte = bitmap.Width % 2 != 0;
                    // If it ends on half a byte, one more needs to be processed.
                    // We subtract in the other case instead, to make it 0-indexed right away.
                    if (!halfByte)
                        lineMax--;
                    for (Int32 i = 0; i < bytes.Length; i++)
                    {
                        // Last position to process.
                        Int32 linepos = i % stride;
                        // Passed last image byte of the line. Abort and go on with loop.
                        if (linepos > lineMax)
                            continue;
                        Byte b = bytes[i];
                        if ((b & 0x0F) == transCol)
                            return true;
                        if (halfByte && linepos == lineMax) // reached last byte of the line. If only half a byte to check on that, abort and go on with loop.
                            continue;
                        if (((b & 0xF0) >> 4) == transCol)
                            return true;
                    }
                }
                return false;
            }
            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb || 
                bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                Byte[] bytes = new Byte[bitmap.Height * data.Stride];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                bitmap.UnlockBits(data);
                for (Int32 p = 3; p < bytes.Length; p += 4)
                {
                    if (bytes[p] != 255)
                        return true;
                }
                return false;
            }
            // Final "screw it all" method. This is pretty slow, but it won't ever be used, unless you
            // encounter some really esoteric types not handled above, like 16bppArgb1555 and 64bppArgb.
            for (Int32 i = 0; i < bitmap.Width; i++)
            {
                for (Int32 j = 0; j < bitmap.Height; j++)
                {
                    if (bitmap.GetPixel(i, j).A != 255)
                        return true;
                }
            }
            return false;
        }
    }
}

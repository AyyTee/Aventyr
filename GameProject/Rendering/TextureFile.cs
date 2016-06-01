using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class TextureFile : ITexture
    {
        /// <summary>
        /// If Filename doesn't point to a valid texture then this is used instead.
        /// </summary>
        readonly static Texture _textureMissing;

        Texture _texture;
        [DataMember]
        public readonly string Filename;

        public TextureFile(string filename)
        {
            Filename = filename;
            LoadImage();
        }

        public int GetId()
        {
            if (_texture == null)
            {
                LoadImage();
            }
            return _texture.GetId();
        }

        private void LoadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            _texture = new Texture(texID);
        }

        private void LoadImage()
        {
            try
            {
                using (Bitmap file = new Bitmap(Filename))
                {
                    LoadImage(file);
                }
            }
            catch (FileNotFoundException)
            {
                Debug.Assert(false, "Texture missing.");
                _texture = _textureMissing;
            }
        }
    }
}

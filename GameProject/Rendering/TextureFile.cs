using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using OpenTK.Graphics.OpenGL;

namespace Game.Rendering
{
    [DataContract]
    public class TextureFile : ITexture
    {
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

        void LoadImage(Bitmap image)
        {
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

            _texture = new Texture(texId);
        }

        void LoadImage()
        {
            try
            {
                using (var file = new Bitmap(Filename))
                {
                    LoadImage(file);
                }
            }
            catch (FileNotFoundException)
            {
                Debug.Assert(false, "Texture missing.");
            }
        }
    }
}

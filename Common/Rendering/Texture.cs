using Game.Common;
using OpenTK;
using System;
using System.Runtime.Serialization;

namespace Game.Rendering
{
    [DataContract]
    public class Texture : IDisposable, ITexture
    {
        public static object LockDelete { get; } = new object();

        [DataMember]
        public bool IsTransparent { get; }

        /// <summary>GL texture id.</summary>
        public int Id { get; private set; }

        public Vector2i Size { get; }

        public RectangleF UvBounds => new RectangleF(new Vector2(), Vector2.One);

        public Texture(int id, Vector2i size, bool isTransparent = false)
        {
            Id = id;
            Size = size;
            IsTransparent = isTransparent;
        }

        public void Dispose()
        {
            if (Id == -1) return;
            lock (LockDelete)
            {
                //throw new NotImplementedException();
                //ResourceController.TextureGarbage.Add(_id);
            }
        }
    }
}

using Game.Common;
using System;
using System.Runtime.Serialization;

namespace Game.Rendering
{
    [DataContract]
    public class Texture : IDisposable, ITexture
    {
        public static object LockDelete { get; } = new object();

        /// <summary>GL texture id.</summary>
        readonly int _id;

        public Vector2i Size { get; }

        public Texture(int id, Vector2i size)
        {
            _id = id;
            Size = size;
        }

        ~Texture()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_id == -1) return;
            lock (LockDelete)
            {
                //throw new NotImplementedException();
                //ResourceController.TextureGarbage.Add(_id);
            }
        }

        public int GetId()
        {
            return _id;
        }
    }
}

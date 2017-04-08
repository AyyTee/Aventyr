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

        public Texture(int id)
        {
            _id = id;
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

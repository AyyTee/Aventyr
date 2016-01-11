using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class Texture : IDisposable, ITexture
    {
        static object _lockDelete = new object();
        public static object LockDelete { get { return _lockDelete; } }
        /// <summary>GL texture id.</summary>
        readonly int _id;

        #region Constructors
        public Texture(int id)
        {
            _id = id;
        }
        #endregion

        ~Texture()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_id != -1)
            {
                lock (LockDelete)
                {
                    Controller.textureGarbage.Add(_id);
                }
            }
        }

        public int GetId()
        {
            return _id;
        }
    }
}

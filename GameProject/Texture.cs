using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Texture : IDisposable
    {
        static object _deleteLock = new object();
        public static object LockDelete { get { return _deleteLock; } }
        public string Filepath { get; private set; }
        /// <summary>
        /// GL texture id
        /// </summary>
        public int Id { get; private set; }

        public Texture(int id)
        {
            Id = id;
        }

        public void SetFilepath(string filepath)
        {
            Filepath = filepath;
        }

        ~Texture()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Id != -1)
            {
                lock (LockDelete)
                {
                    Controller.textureGarbage.Add(Id);
                    Id = -1;
                }
            }
        }
    }
}

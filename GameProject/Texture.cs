using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Texture
    {
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
    }
}

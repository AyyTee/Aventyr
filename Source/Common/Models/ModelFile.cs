using Game;
using Game.Models;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [DataContract]
    public class ModelFile
    {
        ModelGroup _model;

        public string Name => Path.GetFileNameWithoutExtension(Filepath);

        [DataMember]
        public string Filepath { get; private set; }

        public ModelFile(string filepath) => Filepath = filepath;

        public ModelGroup Load(IVirtualWindow window)
        {
            if (_model == null)
            {
                var loadResult = FileFormatWavefront.FileFormatObj.Load(Path.Combine(Resources.ResourcePath, Filepath), false);
                _model = Model.FromWavefront(loadResult.Model, window);
            }
            return _model;
        }

        public void Unload() => _model = null;
    }
}

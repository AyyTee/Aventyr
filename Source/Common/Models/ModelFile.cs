using FileFormatWavefront.Model;
using Game;
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
        Model _model;
        public Model Model
        {
            get
            {
                Load();
                return _model;
            }
            private set => _model = value;
        }

        public string Name => Path.GetFileNameWithoutExtension(Filepath);

        [DataMember]
        public string Filepath { get; private set; }

        public ModelFile(string filepath) => Filepath = filepath;

        public void Load()
        {
            if (_model == null)
            {
                var loadResult = FileFormatWavefront.FileFormatObj.Load(Path.Combine(Resources.ResourcePath, Filepath), false);
                Model = loadResult.Model;
            }
        }

        public void Unload() => Model = null;
    }
}

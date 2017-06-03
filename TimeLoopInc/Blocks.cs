using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using OpenTK.Graphics;
using Game.Rendering;
using OpenTK;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace TimeLoopInc
{
    [DataContract]
    public class Block : IGridEntity
    {
        [DataMember]
        public int Size { get; }
        [DataMember]
        public Vector2i StartPosition { get; }
        [DataMember]
        public int StartTime { get; }
        [DataMember]
        public int EndTime { get; set; }

        public Block(Vector2i startPosition, int startTime, int size = 1)
        {
            Debug.Assert(size > 0);
            StartPosition = startPosition;
            StartTime = startTime;
            Size = size;
        }

        public IGridEntityInstant CreateInstant() => new BlockInstant(StartPosition);

        public IGridEntity DeepClone() => (Block)MemberwiseClone();

        public List<Model> GetModels()
        {
            var model = ModelFactory.CreatePlane(Vector2.One * Size, new Vector3(-Size / 2));
            model.SetColor(new Color4(0.5f, 1f, 0.8f, 1f));
            return new List<Model>() { model };
        }
    }
}

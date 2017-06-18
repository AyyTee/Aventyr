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
        public Transform2i StartTransform { get; }
        [DataMember]
        public Vector2i PreviousVelocity { get; }
        [DataMember]
        public int StartTime { get; }
        [DataMember]
        public int EndTime { get; set; } = int.MaxValue;

        public Block(Transform2i startTransform, int startTime, Vector2i previousVelocity = new Vector2i())
        {
            StartTransform = startTransform;
            PreviousVelocity = previousVelocity;
            StartTime = startTime;
        }

        public IGridEntityInstant CreateInstant() => new BlockInstant(StartTransform, PreviousVelocity);

        public IGridEntity DeepClone() => (Block)MemberwiseClone();

        public List<Model> GetModels()
        {
            var model = ModelFactory.CreatePlane(Vector2.One * StartTransform.Size, new Vector3(-StartTransform.Size / 2));
            model.SetColor(new Color4(0.5f, 1f, 0.8f, 1f));
            return new List<Model>() { model };
        }
    }
}

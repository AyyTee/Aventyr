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
        public Transform2i PreviousTransform { get; }
        [DataMember]
        public int PreviousTime { get; }
        [DataMember]
        public int StartTime { get; }


        public Block()
            : this(new Transform2i())
        {
        }

        public Block(Transform2i startTransform, int startTime = int.MinValue)
        {
            StartTransform = startTransform;
            StartTime = startTime;
        }

        public Block(
            Transform2i startTransform,
            int startTime,
            Vector2i previousVelocity,
            Transform2i previousTransform,
            int previousTime)
            : this(startTransform, startTime)
        {
            PreviousVelocity = previousVelocity;
            PreviousTransform = previousTransform;
            PreviousTime = previousTime;
        }

        public IGridEntityInstant CreateInstant() => new BlockInstant(StartTransform, PreviousVelocity);

        public IGridEntity DeepClone() => (Block)MemberwiseClone();

        public List<Model> GetModels()
        {
            var model = ModelFactory.CreatePlane(
                Vector2.One * StartTransform.Size, 
                new Color4(0.5f, 1f, 0.8f, 1f), 
                new Vector3(-StartTransform.Size / 2));
            return new List<Model> { model };
        }
    }
}

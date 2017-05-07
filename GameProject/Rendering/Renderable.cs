using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using System.Runtime.Serialization;

namespace Game.Rendering
{
    [DataContract]
    public class Renderable : IRenderable, ITransformable2
    {
        [DataMember]
        public bool Visible { get; set; } = true;

        [DataMember]
        public bool DrawOverPortals { get; set; }

        [DataMember]
        public bool IsPortalable { get; set; } = true;

        [DataMember]
        Transform2 _transform = new Transform2();
        public Transform2 WorldTransform => _transform.ShallowClone();
        public Transform2 WorldVelocity => Transform2.CreateVelocity();

        [DataMember]
        public List<Model> Models { get; set; } = new List<Model>();

        public List<Model> GetModels() => new List<Model>(Models);

        public void SetTransform(Transform2 transform) => _transform = transform.ShallowClone();
        public Transform2 GetTransform() => _transform.ShallowClone();
    }
}

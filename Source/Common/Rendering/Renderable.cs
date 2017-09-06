﻿using System;
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
    public class Renderable : IRenderable
    {
        [DataMember]
        public bool Visible { get; set; } = true;

        [DataMember]
        public bool DrawOverPortals { get; set; }

        [DataMember]
        public bool IsPortalable { get; set; } = true;

        [DataMember]
        public bool PixelAlign { get; set; }

        [DataMember]
        public Transform2 WorldTransform { get; set; } = new Transform2();
        [DataMember]
        public Transform2 WorldVelocity { get; set; } = Transform2.CreateVelocity();

        [DataMember]
        public List<Model> Models { get; set; } = new List<Model>();
        [DataMember]
        public List<ClipPath> ClipPaths { get; set; } = new List<ClipPath>();

        public List<Model> GetModels() => new List<Model>(Models);

        public Renderable()
        {
        }

        public Renderable(Transform2 worldTransform)
        {
            WorldTransform = worldTransform;
        }

        public Renderable(Model model)
        {
            Models.Add(model);
        }

        public Renderable(Transform2 worldTransform, List<Model> models)
        {
            WorldTransform = worldTransform;
            Models = models;
        }
    }
}

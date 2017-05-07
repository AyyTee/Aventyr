using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Models;
using OpenTK;
using OpenTK.Graphics;

namespace TimeLoopInc
{
    public class Renderable : IRenderable
    {
        public bool Visible => true;
        public bool DrawOverPortals => false;
        public bool IsPortalable { get; set; } = true;
        public Color4 Color { get; set; }
        public float Size { get; set; } = 1;

        readonly Vector2 _position;

        public Renderable(Vector2 position)
        {
            _position = position;
        }

        public Renderable(Vector2i position)
        {
            _position = (Vector2)position;
        }

        public List<Model> GetModels()
        {
            var model = ModelFactory.CreatePlane(Vector2.One * Size, new Vector3(-Size / 2));
            model.SetColor(Color);
            return new List<Model>() { model };
        }

        public Transform2 GetWorldTransform(bool ignorePortals = false) => new Transform2(_position + Vector2.One * 0.5f * Size);

        public Transform2 GetWorldVelocity(bool ignorePortals = false) => Transform2.CreateVelocity();
    }
}

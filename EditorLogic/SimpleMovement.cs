using Game;
using Game.Portals;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Rendering;

namespace EditorLogic
{
    public class SimpleMovement
    {
        public float Acceleration { get; set; }
        /// <summary>Friction coefficient.</summary>
        public float Friction { get; set; }
        public IVirtualWindow Input { get; set; }

        public SimpleMovement(IVirtualWindow input, float acceleration, float friction)
        {
            Acceleration = acceleration;
            Friction = friction;
            Input = input;
        }

        public void Move(IPortalable moveable)
        {
            Transform2 v = moveable.GetVelocity();
            Transform2 t = moveable.GetTransform();
            if (Input.ButtonDown(Key.Left))
            {
                v.Position += t.GetRight() * -Acceleration * Math.Abs(moveable.GetTransform().Size);
            }
            if (Input.ButtonDown(Key.Right))
            {
                v.Position += t.GetRight() * Acceleration * Math.Abs(moveable.GetTransform().Size);
            }
            if (Input.ButtonDown(Key.Up))
            {
                v.Position += t.GetUp() * Acceleration * Math.Abs(moveable.GetTransform().Size);
            }
            if (Input.ButtonDown(Key.Down))
            {
                v.Position += t.GetUp() * -Acceleration * Math.Abs(moveable.GetTransform().Size);
            }
            v.Position *= 1 - Friction;
            moveable.SetVelocity(v);
        }
    }
}

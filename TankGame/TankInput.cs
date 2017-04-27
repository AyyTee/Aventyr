using Game.Rendering;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    [DataContract]
    public class TankInput
    {
        [DataMember]
        public bool MoveFoward;
        [DataMember]
        public bool MoveBackward;
        [DataMember]
        public bool TurnLeft;
        [DataMember]
        public bool TurnRight;
        /// <summary>
        /// Aiming reticle position in world coordinates.
        /// </summary>
        [DataMember]
        public Vector2 ReticlePos;
        [DataMember]
        public readonly bool[] FirePortal = new bool[2];
        public bool FirePortalLeft {
            get { return FirePortal[0]; }
            set { FirePortal[0] = value; }
        }
        public bool FirePortalRight
        {
            get { return FirePortal[1]; }
            set { FirePortal[1] = value; }
        }
        [DataMember]
        public bool FireGun;

        public static TankInput CreateInput(IVirtualWindow window, ICamera2 camera)
        {
            return new TankInput
            {
                MoveFoward = window.Input.KeyDown(Key.W),
                MoveBackward = window.Input.KeyDown(Key.S),
                TurnLeft = window.Input.KeyDown(Key.A),
                TurnRight = window.Input.KeyDown(Key.D),
                ReticlePos = window.Input.GetMouseWorldPos(camera, (Vector2)window.CanvasSize),
                FireGun = window.Input.KeyPress(Key.Space),
                FirePortalLeft = window.Input.MousePress(MouseButton.Left),
                FirePortalRight = window.Input.MousePress(MouseButton.Right)
            };
        }
    }
}

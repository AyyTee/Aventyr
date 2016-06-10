using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Keyframe
    {
        public float Time;
        public float Value;
        public HandleType Handle = HandleType.Vector;

        public enum HandleType
        {
            Vector,
            Spline
        }

        public Keyframe(float time, float value)
        {
            Value = value;
            Time = time;
        }
    }
}

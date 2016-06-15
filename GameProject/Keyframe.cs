using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class Keyframe : IShallowClone<Keyframe>
    {
        [DataMember]
        public readonly float Time;
        [DataMember]
        public float Value;
        [DataMember]
        public InterpolateType Handle;

        public enum InterpolateType
        {
            Linear,
            Spline,
            Constant
        }

        public Keyframe()
            : this(0, 0, InterpolateType.Linear)
        {
        }

        public Keyframe(float time, float value)
            : this(time, value, InterpolateType.Linear)
        {
        }

        public Keyframe(float time, float value, InterpolateType handle)
        {
            Time = time;
            Value = value;
            Handle = handle;
        }

        public Keyframe ShallowClone()
        {
            return new Keyframe(Time, Value, Handle);
        }
    }
}

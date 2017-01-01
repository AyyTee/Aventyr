using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Serialization;

namespace Game.Animation
{
    [DataContract]
    public class Curve : IShallowClone<Curve>
    {
        [DataMember]
        public float DefaultValue { get; set; }
        [DataMember]
        public bool IsLoop { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public SortedList<float, Keyframe> Keyframes = new SortedList<float, Keyframe>();
        public float Length { get { return Keyframes.Count == 0 ? 0 : Keyframes.Last().Key; } }

        public Curve()
        {
        }

        public Curve(float defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public float GetValue(float time)
        {
            if (Keyframes.Count == 0)
            {
                return DefaultValue;
            }
            else if (Keyframes.Count == 1)
            {
                return Keyframes[0].Value;
            }
            if (IsLoop)
            {
                time = (float)MathExt.ValueWrap(time, Length);
            }
            else if (time < Keyframes.First().Key)
            {
                return Keyframes.First().Value.Value;
            }
            else if (time >= Length)
            {
                return Keyframes.Last().Value.Value;
            }
            for (int i = -1; i < Keyframes.Count; i++)
            {
                Keyframe next = Keyframes.ElementAt((i + 1) % Keyframes.Count).Value;
                if (time < next.Time)
                {
                    return _getValue((i + Keyframes.Count) % Keyframes.Count, time);
                }
            }
            Debug.Fail("Execution should not have reached this point.");
            return 0;
        }

        private float _getValue(int index, float time)
        {
            Keyframe current = Keyframes.ElementAt(index).Value;
            if (IsLoop || (index >= 0 && index + 1 < Keyframes.Count))
            {
                Keyframe next = Keyframes.ElementAt((index + 1) % Keyframes.Count).Value;
                float nextTime = next.Time < current.Time ? next.Time + Length : next.Time;
                if (time == nextTime)
                {
                    return current.Value;
                }
                float t = (time - current.Time) / (nextTime - current.Time);
                return (float)MathExt.Lerp(current.Value, next.Value, t);
            }
            return current.Value;
        }

        /// <summary>
        /// Add a new keyframe. If one already exists at the given time then it is overwritten.
        /// </summary>
        public void AddKeyframe(Keyframe keyframe)
        {
            Debug.Assert(keyframe != null);
            if (Keyframes.ContainsKey(keyframe.Time))
            {
                Keyframes.Remove(keyframe.Time);
            }
            Keyframes.Add(keyframe.Time, keyframe);
        }

        public float GetDerivative(float time)
        {
            if (Keyframes.Count == 0)
            {
                return 0;
            }
            if (IsLoop)
            {
                time = (float)MathExt.ValueWrap(time, Length);
            }
            else if (time < Keyframes.First().Key)
            {
                return 0;
            }
            else if (time >= Length)
            {
                return 0;
            }
            for (int i = 0; i < Keyframes.Count; i++)
            {
                Keyframe next = Keyframes.ElementAt((i + 1) % Keyframes.Count).Value;
                if (time < next.Time)
                {
                    return _getDerivative(i, time);
                }
            }
            Debug.Fail("Execution should not have reached this point.");
            return 0;
        }

        private float _getDerivative(int index, float time)
        {
            Keyframe current = Keyframes.ElementAt(index).Value;
            if (IsLoop || (index >= 0 && index + 1 < Keyframes.Count))
            {
                Keyframe next = Keyframes.ElementAt((index + 1) % Keyframes.Count).Value;
                float nextTime = next.Time < current.Time ? next.Time + Length : next.Time;
                return (next.Value - current.Value) / (nextTime - current.Time);
            }
            return current.Value;
        }

        /// <summary>
        /// Create a shallow copy of this instance.  Note that this will however deep copy Keyframes.
        /// </summary>
        /// <returns></returns>
        public Curve ShallowClone()
        {
            Curve clone = new Curve();
            clone.DefaultValue = DefaultValue;
            foreach (Keyframe k in Keyframes.Values)
            {
                clone.AddKeyframe(k);
            }
            return clone;
        }
    }
}

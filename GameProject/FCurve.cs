using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class FCurve
    {
        public float DefaultValue;
        public bool IsLoop;
        public string Name;
        SortedList<float, Keyframe> _keyframes = new SortedList<float, Keyframe>();
        public float Length { get { return _keyframes.Count == 0 ? 0 : _keyframes.Last().Key; } }

        public FCurve()
        {
        }

        public FCurve(float defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public float GetValue(float time)
        {
            if (_keyframes.Count == 0)
            {
                return DefaultValue;
            }
            if (IsLoop)
            {
                time = (float)MathExt.ValueWrap(time, Length);
            }
            else if (time < _keyframes.First().Key)
            {
                return _keyframes.First().Value.Value;
            }
            else if (time >= Length)
            {
                return _keyframes.Last().Value.Value;
            }
            for (int i = -1; i < _keyframes.Count; i++)
            {
                Keyframe next = _keyframes.ElementAt((i + 1) % _keyframes.Count).Value;
                if (time < next.Time)
                {
                    return _getValue((i + _keyframes.Count) % _keyframes.Count, time);
                }
            }
            Debug.Fail("Execution should not have reached this point.");
            return 0;
        }

        private float _getValue(int index, float time)
        {
            Keyframe current = _keyframes.ElementAt(index).Value;
            if (IsLoop || (index >= 0 && index + 1 < _keyframes.Count))
            {
                Keyframe next = _keyframes.ElementAt((index + 1) % _keyframes.Count).Value;
                float nextTime = next.Time < current.Time ? next.Time + Length : next.Time;
                float t = (time - current.Time) / (nextTime - current.Time);
                return (float)MathExt.Lerp(current.Value, next.Value, t);
            }
            return current.Value;
        }

        public void AddKeyframe(Keyframe keyframe)
        {
            Debug.Assert(keyframe != null);
            Debug.Assert(keyframe.Time > 0, "Keyframe must have a time value greater than 0.");
            Debug.Assert(!_keyframes.ContainsKey(keyframe.Time));
            _keyframes.Add(keyframe.Time, keyframe);
        }
    }
}

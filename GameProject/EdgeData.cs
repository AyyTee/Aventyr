using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class EdgeData<T>
    {
        private SortedList<float, Segment> _segments = new SortedList<float, Segment>();

        private class Segment
        {
            public float Begin { get; private set; }
            public float End { get; private set; }
            public T data { get; set; }

            public Segment(T data, float begin, float end)
            {
                Debug.Assert(begin <= end);
                Begin = begin;
                End = end;
            }
        }

        public EdgeData()
        {
        }

        public void AddSegment(T data, float begin, float end)
        {
            _segments.Add(begin, new Segment(data, begin, end));
        }

        /// <summary>
        /// Removes the first instance that matches the data instance.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if data exists.</returns>
        public bool RemoveSegment(T data)
        {
            for (int i = 0; i < _segments.Count(); i++)
            {
                if (_segments[i].data.Equals(data))
                {
                    _segments.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /*public IEnumerator<T> GetEnunmerator()
        {
            var aa = _segments.GetEnumerator();
            //return 
        }*/
    }
}

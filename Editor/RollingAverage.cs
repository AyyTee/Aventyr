using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class RollingAverage
    {
        Queue<float> _queue = new Queue<float>();

        public RollingAverage(int size, float value)
        {
            for (int i = 0; i < size; i++)
            {
                _queue.Enqueue(value);
            }
        }

        public void Enqueue(float value)
        {
            lock (this)
            {
                _queue.Enqueue(value);
                _queue.Dequeue();
            }
        }

        public float GetAverage()
        {
            lock (this)
            {
                return _queue.Average();
            }
        }
    }
}

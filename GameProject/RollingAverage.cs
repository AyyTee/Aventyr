using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class RollingAverage
    {
        public Queue<float> Queue = new Queue<float>();

        public RollingAverage(int size, float startValue)
        {
            for (int i = 0; i < size; i++)
            {
                Queue.Enqueue(startValue);
            }
        }

        public void Enqueue(float value)
        {
            lock (this)
            {
                Queue.Enqueue(value);
                Queue.Dequeue();
            }
        }

        public float GetAverage()
        {
            lock (this)
            {
                return Queue.Average();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    public static class TimeSpanEx
    {
        public static double Div(this TimeSpan timeSpan, TimeSpan divisor)
        {
            return timeSpan.TotalSeconds / divisor.TotalSeconds;
        }
    }
}

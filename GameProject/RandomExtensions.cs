using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /// <summary>
    /// Extends Random to support creating random longs.
    /// </summary>
    /// <remarks>Class found here http://stackoverflow.com/a/6651656
    /// </remarks>
    public static class RandomExtensions
    {
        public static long NextLong(this Random rnd)
        {
            byte[] buffer = new byte[8];
            rnd.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static long NextLong(this Random rnd, long min, long max)
        {
            EnsureMinLEQMax(ref min, ref max);
            long numbersInRange = unchecked(max - min + 1);
            if (numbersInRange < 0)
                throw new ArgumentException("Size of range between min and max must be less than or equal to Int64.MaxValue");

            long randomOffset = NextLong(rnd);
            if (IsModuloBiased(randomOffset, numbersInRange))
                return NextLong(rnd, min, max); // Try again
            else
                return min + PositiveModuloOrZero(randomOffset, numbersInRange);
        }

        static bool IsModuloBiased(long randomOffset, long numbersInRange)
        {
            long greatestCompleteRange = numbersInRange * (long.MaxValue / numbersInRange);
            return randomOffset > greatestCompleteRange;
        }

        static long PositiveModuloOrZero(long dividend, long divisor)
        {
            long mod;
            Math.DivRem(dividend, divisor, out mod);
            if (mod < 0)
                mod += divisor;
            return mod;
        }

        static void EnsureMinLEQMax(ref long min, ref long max)
        {
            if (min <= max)
                return;
            long temp = min;
            min = max;
            max = temp;
        }
    }
}

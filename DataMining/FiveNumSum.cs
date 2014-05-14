using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoardGameGeek
{
    internal class FiveNumSum<T> where T:IComparable
    {
        internal T Min { get; private set; }
        internal T Max { get; private set; }
        internal T SeventyFive { get; private set; }
        internal T TwentyFive { get; private set; }
        internal T Median { get; private set; }

        private FiveNumSum(T min, T max, T sf, T tf, T median)
        {
            Min = min;
            Max = max;
            SeventyFive = sf;
            TwentyFive = tf;
            Median = median;
        }


        internal static FiveNumSum<T> GetFiveNumSum(T[] data)
        {
            Array.Sort(data);

            T min = data.First();
            T max = data.Last();
            int medianindex = (int) Math.Round(((double) data.Length)/2);
            int threefourths = (int) Math.Round(((double) data.Length*3)/4);
            T median = data[medianindex];
            T sf = data[threefourths];
            T tf = data[(data.Length - threefourths)];

            return new FiveNumSum<T>(min,max,sf,tf,median);
        }

        public override string ToString()
        {
            return Min +
                   "\t& " + TwentyFive +
                   "\t& " + Median +
                   "\t& " + SeventyFive +
                   "\t& " + Max;
            /*return "Min: "+Min+
                   ",\tQ1: "+TwentyFive+
                   ",\tQ2: "+Median+
                   ",\tQ3: "+SeventyFive+
                   ",\tMax: "+Max;*/
        }
    }
}

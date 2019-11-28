using System.Collections.Generic;
using System.Linq;

namespace Monik.Service
{
    public class AccumWindowCalculator : IWindowCalculator
    {
        private readonly LinkedList<double> queue;
        private double accum;

        public AccumWindowCalculator()
        {
            var arr = Enumerable.Range(1, 300).Select(x => (double)0);
            queue = new LinkedList<double>(arr);
            accum = 0;
        }

        public void OnNewValue(double value)
        {
            lock (this)
            {
                queue.First.Value += value;
                accum += value;
            }
        }

        // TODO: remove skipped seconds

        public void OnNextSecond()
        {
            lock (this)
            {
                if (queue.Count != 0)
                {
                    var last = queue.Last.Value;

                    queue.RemoveLast();

                    accum -= last;
                }

                queue.AddFirst((double)0);
            }//lock
        }

        public double GetValue()
        {
            lock (this)
                return accum;
        }
    }
}
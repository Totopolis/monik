using System.Collections.Generic;
using System.Linq;

namespace Monik.Service
{
    public interface IWindowCalculator
    {
        void OnNewValue(double value);
        void OnNextSecond();
        double GetValue();
    }

    public class GaugeWindowCalculator : IWindowCalculator
    {
        private readonly LinkedList<double?> queue;
        private double accum;
        private int counter;

        public GaugeWindowCalculator()
        {
            var arr = Enumerable.Range(1, 300).Select(x => (double?)null);
            queue = new LinkedList<double?>(arr);

            accum = 0;
            counter = 0;
        }

        public void OnNewValue(double value)
        {
            lock (this)
            {
                if (!queue.First.Value.HasValue)
                {
                    counter++;
                    queue.First.Value = value;
                    accum += value;
                }
                else
                {
                    double prevValue = queue.First.Value.Value;
                    double newValue = (prevValue + value) / 2;

                    queue.First.Value += newValue;

                    accum -= prevValue;
                    accum += newValue;
                }
            }//lock
        }

        public void OnNextSecond()
        {
            lock (this)
            {
                if (queue.Count != 0)
                {
                    var last = queue.Last.Value;

                    if (last.HasValue)
                    {
                        accum -= last.Value;
                        counter--;
                    }

                    queue.RemoveLast();
                }

                queue.AddFirst((double?)null);
            }//lock
        }

        public double GetValue() => counter == 0 ? 0 : (accum / counter);

    }//end of class

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

        public double GetValue() => accum;
    }//end of class
}

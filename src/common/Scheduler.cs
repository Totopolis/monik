using Monik.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Monik.Common
{
    public class Scheduler
    {
        public readonly CancellationTokenSource Cancellation = new CancellationTokenSource();
        private readonly Action _action;

        public enum IntervalType
        {
            Minute,
            Hour,
            Day
        }

        public IntervalType Interval { get; private set; }
        private readonly string _name;
        private readonly IClientControl _control;

        // TODO: options - retry logic and immediatly execute at start
        private Scheduler(IClientControl aControl, Action aWork, IntervalType aInterval, string aName)
        {
            _control = aControl;
            _action = aWork;
            _name = aName;
            Interval = aInterval;
        }

        public void OnStart()
        {
            Runner(DateTime.Now.AddSeconds(1));
        }

        public void OnStop()
        {
            Cancellation.Cancel();
        }

        private DateTime GetNextDate(DateTime aDate)
        {
            switch (Interval)
            {
                case IntervalType.Minute:
                    return new DateTime(aDate.Year, aDate.Month, aDate.Day, aDate.Hour, aDate.Minute, 0).AddMinutes(1);
                case IntervalType.Hour:
                    return new DateTime(aDate.Year, aDate.Month, aDate.Day, aDate.Hour, 0, 0).AddHours(1);
                case IntervalType.Day:
                    return new DateTime(aDate.Year, aDate.Month, aDate.Day).AddDays(1);
                default:
                    throw new NotImplementedException();
            }
        }

        private void Runner(DateTime date)
        {
            var dateNow = DateTime.Now;
            TimeSpan ts;
            if (date > dateNow)
                ts = date - dateNow;
            else
            {
                date = GetNextDate(date);
                ts = date - dateNow;
            }

            // for deviation
            ts = ts.Add(TimeSpan.FromSeconds(1));
            //M.ApplicationInfo("Scheduler {0} delay: {1}ms", _name, ts.TotalMilliseconds);

            //waits certn time and run the code, in meantime yuo can cancel the task at any time
            Task.Delay(ts).ContinueWith((x) =>
            {
                try
                {
                    _action();
                }
                catch (Exception ex)
                {
                    _control.ApplicationError($"Scheduler {_name} exception: {ex.Message}");
                }

                Runner(GetNextDate(date));

            }, Cancellation.Token);
        }

        public static Scheduler CreatePerHour(IClientControl aControl, Action aWork, string aName = "")
        {
            Scheduler result = new Scheduler(aControl, aWork, IntervalType.Hour, aName);
            return result;
        }

    } //end of class
}

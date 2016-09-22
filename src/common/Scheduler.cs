using Monik.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Monik.Common
{
  public class Scheduler
  {
    public readonly CancellationTokenSource Cancellation = new CancellationTokenSource();
    private Action FAction;

    public enum IntervalType
    {
      Minute,
      Hour,
      Day
    }

    public IntervalType Interval { get; private set; }
    private string FName;

    // TODO: options - retry logic and immediatly execute at start
    private Scheduler(Action aWork, IntervalType aInterval, string aName)
    {
      FAction = aWork;
      FName = aName;
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

    private DateTime getNextDate(DateTime aDate)
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
        date = getNextDate(date);
        ts = date - dateNow;
      }

      M.ApplicationInfo("Scheduler {0} delay: {1}ms", FName, ts.TotalMilliseconds);

      //waits certn time and run the code, in meantime yuo can cancel the task at any time
      Task.Delay(ts).ContinueWith((x) =>
      {
        try
        {
          FAction();
        }
        catch(Exception _e)
        {
          M.ApplicationError("Scheduler {0} exception: {1}", FName, _e.Message);
        }

        Runner(getNextDate(date));

      }, Cancellation.Token);
    }

    public static Scheduler CreatePerHour(Action aWork, string aName = "")
    {
      Scheduler _res = new Scheduler(aWork, IntervalType.Hour, aName);
      return _res;
    }

  }//end of class
}

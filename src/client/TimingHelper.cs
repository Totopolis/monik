using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Threading;
using Monik.Common;
using System.Runtime.CompilerServices;

namespace Monik.Client
{
  public class TimingHelper
  {
    private DateTime FFrom = DateTime.Now;
    private IClientControl FControl;

    private TimingHelper(IClientControl aControl)
    {
      FControl = aControl;
    }

    public static TimingHelper Create(IClientControl aControl) { return new TimingHelper(aControl); }

    public void Begin() { FFrom = DateTime.Now; }

    public void EndAndLog([CallerMemberName] string aSource = "")
    {
      var _delta = DateTime.Now - FFrom;
      FControl.ApplicationInfo("{0} execution time: {1}ms", aSource, _delta.TotalMilliseconds);
    }
  }
}

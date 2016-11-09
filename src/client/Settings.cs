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

namespace Monik.Client
{
  public class ClientSettings : IClientSettings
  {
    /*private BehaviorSubject<string> FSourceNameProperty = new BehaviorSubject<string>("UnknownSource");
    private BehaviorSubject<string> FInstanceNameProperty = new BehaviorSubject<string>("UnknownInstance");
    private BehaviorSubject<ushort> FSendDelayProperty = new BehaviorSubject<ushort>(1);
    private BehaviorSubject<bool> FAutoKeepAliveEnableProperty = new BehaviorSubject<bool>(false);
    private BehaviorSubject<ushort> FAutoKeepAliveIntervalProperty = new BehaviorSubject<ushort>(60);

    public IObservable<string> SourceNameProperty { get { return FSourceNameProperty; } }
    public IObservable<string> InstanceNameProperty { get { return FInstanceNameProperty; } }
    public IObservable<ushort> SendDelayProperty { get { return FSendDelayProperty; } }
    public IObservable<bool> AutoKeepAliveEnableProperty { get { return FAutoKeepAliveEnableProperty; } }
    public IObservable<ushort> AutoKeepAliveIntervalProperty { get { return FAutoKeepAliveIntervalProperty; } }*/

    public string SourceName { get; set; }

    public string InstanceName { get; set; }

    public ushort SendDelay { get; set; }

    public bool AutoKeepAliveEnable { get; set; }

    public ushort AutoKeepAliveInterval { get; set; }

  }//end of class
}

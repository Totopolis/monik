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
using System.Reactive.Subjects;

namespace Monik.Client
{
  public class ClientSettings : IClientSettings
  {
    private BehaviorSubject<string> FSourceNameProperty = new BehaviorSubject<string>("UnknownSource");
    private BehaviorSubject<string> FInstanceNameProperty = new BehaviorSubject<string>("UnknownInstance");
    private BehaviorSubject<ushort> FSendDelayProperty = new BehaviorSubject<ushort>(1);
    private BehaviorSubject<bool> FAutoKeepAliveEnableProperty = new BehaviorSubject<bool>(false);
    private BehaviorSubject<ushort> FAutoKeepAliveIntervalProperty = new BehaviorSubject<ushort>(60);

    public IObservable<string> SourceNameProperty { get { return FSourceNameProperty; } }
    public IObservable<string> InstanceNameProperty { get { return FInstanceNameProperty; } }
    public IObservable<ushort> SendDelayProperty { get { return FSendDelayProperty; } }
    public IObservable<bool> AutoKeepAliveEnableProperty { get { return FAutoKeepAliveEnableProperty; } }
    public IObservable<ushort> AutoKeepAliveIntervalProperty { get { return FAutoKeepAliveIntervalProperty; } }

    public string SourceName {
      get { return FSourceNameProperty.Value; }
      set
      {
        if (FSourceNameProperty.Value != value)
          FSourceNameProperty.OnNext(value);
      }
    }

    public string InstanceName
    {
      get { return FInstanceNameProperty.Value; }
      set
      {
        if (FInstanceNameProperty.Value != value)
          FInstanceNameProperty.OnNext(value);
      }
    }

    public ushort SendDelay
    {
      get { return FSendDelayProperty.Value; }
      set
      {
        if (FSendDelayProperty.Value != value)
          FSendDelayProperty.OnNext(value);
      }
    }

    public bool AutoKeepAliveEnable
    {
      get { return FAutoKeepAliveEnableProperty.Value; }
      set
      {
        if (FAutoKeepAliveEnableProperty.Value != value)
          FAutoKeepAliveEnableProperty.OnNext(value);
      }
    }

    public ushort AutoKeepAliveInterval
    {
      get { return FAutoKeepAliveIntervalProperty.Value; }
      set
      {
        if (FAutoKeepAliveIntervalProperty.Value != value)
          FAutoKeepAliveIntervalProperty.OnNext(value);
      }
    }

  }//end of class
}

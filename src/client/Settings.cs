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

        public string SourceName { get; set; } = "UnknownSource";

        public string InstanceName { get; set; } = "UnknownInstance";

        public ushort SendDelay { get; set; } = 1;

        public bool AutoKeepAliveEnable { get; set; } = false;

        public ushort AutoKeepAliveInterval { get; set; } = 60;
    } //end of class
}
namespace Monik.Client.Settings
{
    public class ClientSettings : IClientSettings
    {
        public string SourceName { get; set; } = "UnknownSource";

        public string InstanceName { get; set; } = "UnknownInstance";

        public ushort SendDelay { get; set; } = 1;

        public bool AutoKeepAliveEnable { get; set; } = false;

        public ushort AutoKeepAliveInterval { get; set; } = 60;
    } //end of class
}
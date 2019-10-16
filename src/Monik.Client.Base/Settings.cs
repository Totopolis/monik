namespace Monik.Client
{
    public class ClientSettings : IMonikSettings
    {
        public string SourceName { get; set; } = "UnknownSource";

        public string InstanceName { get; set; } = "UnknownInstance";

        public ushort SendDelay { get; set; } = 1;

        public int WaitTimeOnStop { get; set; } = 10_000;

        public bool AutoKeepAliveEnable { get; set; } = false;

        public ushort AutoKeepAliveInterval { get; set; } = 60;

        public bool GroupDuplicates { get; set; } = true;
    } //end of class
}

namespace Monik.Client.Settings
{
    public interface IClientSettings
    {
        string SourceName            { get; set; }
        string InstanceName          { get; set; }
        ushort SendDelay             { get; set; }
        ushort AutoKeepAliveInterval { get; set; }
        bool   AutoKeepAliveEnable   { get; set; }

        // TODO: logging level cut support
    }
}
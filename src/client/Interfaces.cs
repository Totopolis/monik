using System.Collections.Concurrent;
using Monik.Common;

namespace Monik.Client
{
    public interface IClientSender
    {
        void SendMessages(ConcurrentQueue<Event> aQueue);
    }

    public interface IMonikSettings
    {
        /// <summary>
        /// Name of the source (app name, service or smthing else)
        /// </summary>
        string SourceName { get; set; }

        /// <summary>
        /// Name of the concrete instance (production, development, instace number, etc...)
        /// </summary>
        string InstanceName { get; set; }

        /// <summary>
        /// How many seconds to wait after execute monik method, but before message will be send
        /// </summary>
        ushort SendDelay { get; set; }

        /// <summary>
        /// If AutoKeepAlive enabled, KeepAlive() will be raised per each interval (in seconds)
        /// </summary>
        ushort AutoKeepAliveInterval { get; set; }

        /// <summary>
        /// Enable/disable automatic KeepAlive() method executor
        /// </summary>
        bool AutoKeepAliveEnable { get; set; }

        // TODO: logging level client cut support
    }
}

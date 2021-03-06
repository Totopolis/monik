﻿namespace Monik.Client
{
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
        /// How many milliseconds to wait for completion after OnStop call
        /// NOTE: -1 milliseconds represents an infinite time-out.
        /// </summary>
        int WaitTimeOnStop { get; set; }

        /// <summary>
        /// If AutoKeepAlive enabled, KeepAlive() will be raised per each interval (in seconds)
        /// </summary>
        ushort AutoKeepAliveInterval { get; set; }

        /// <summary>
        /// Enable/disable automatic KeepAlive() method executor
        /// </summary>
        bool AutoKeepAliveEnable { get; set; }

        /// <summary>
        /// If enabled Logs and KeepAlives will be grouped,
        /// Also KeepAlives and Metrics will be prioritised
        /// </summary>
        bool GroupDuplicates { get; set; }

        /// <summary>
        /// Capacity of the events queue, old events are discarded
        /// </summary>
        int QueueCapacity { get; set; }
    }
}
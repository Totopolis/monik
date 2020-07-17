using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using EasyNetQ;

namespace Monik.Common.RabbitMQ
{
    public static class RabbitConnectionConfigurationExtensions
    {
        private static readonly Dictionary<string, Action<string, ConnectionConfiguration>> SslFields = new
            Dictionary<string, Action<string, ConnectionConfiguration>>
            {
                {
                    "Ssl.Enabled",
                    (value, config) => config.Ssl.Enabled =
                        value.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase)
                },
                {
                    "Ssl.ServerName",
                    (value, config) => config.Ssl.ServerName = value
                },
                {
                    "Ssl.CertPath",
                    (value, config) => config.Ssl.CertPath = value
                },
                {
                    "Ssl.CertPassphrase",
                    (value, config) => config.Ssl.CertPassphrase = value
                },
                {
                    "Ssl.Version",
                    (value, config) => config.Ssl.Version = Enum.TryParse(value, out SslProtocols v)
                        ? v
                        : throw new ArgumentException($"Wrong Ssl.Version {value}")
                },
            };

        public static string FetchConnectionSslOptions(this string connectionString, out Func<ConnectionConfiguration, ConnectionConfiguration> configure)
        {
            var settings = connectionString
                .Split(';')
                .Select(x => x.Split(new[] { '=' }, 2))
                .ToDictionary(x => x[0], x => x[1]);

            var actions = new List<Action<ConnectionConfiguration>>();

            var sslSettings = settings.Where(pair => SslFields.ContainsKey(pair.Key)).ToList();
            foreach (var pair in sslSettings)
            {
                actions.Add(config => SslFields[pair.Key](pair.Value, config));
                settings.Remove(pair.Key);
            }

            configure = config =>
            {
                foreach (var action in actions)
                    action(config);

                return config;
            };

            return string.Join(";", settings.Select(x => $"{x.Key}={x.Value}"));
        }
    }
}
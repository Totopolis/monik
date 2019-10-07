using Nancy;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Monik.Service
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonNetSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                // default serializer threats all DataTime with Kind.Unspecified as Local time
                // 2018-09-19T16:10:00 will be serialized with server local time - 2018-09-19T16:10:00.0000000+03:00
                // because of that JSON.NET with setting DateTimeZoneHandling.Utc is used, to properly handle timezones
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            _serializer = JsonSerializer.Create(settings);
        }

        public bool CanSerialize(string contentType)
        {
            return contentType == "application/json";
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var writer = new JsonTextWriter(new StreamWriter(outputStream)))
            {
                _serializer.Serialize(writer, model);
                writer.Flush();
            }
        }

        public IEnumerable<string> Extensions => Enumerable.Empty<string>();
    }
}
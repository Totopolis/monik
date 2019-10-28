using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace Monik.Common
{
    internal static class ChannelReaderExtensions
    {
        private static IEnumerable<T> ReadAll<T>(this ChannelReader<T> reader)
        {
            while (reader.TryRead(out var item))
                yield return item;
        }

        public static bool TryReadAll<T>(this ChannelReader<T> reader, out IList<T> items)
        {
            items = reader.ReadAll().ToList();
            return items.Any();
        }
    }
}
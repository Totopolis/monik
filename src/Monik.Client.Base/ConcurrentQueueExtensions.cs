using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Monik.Common
{
    public static class ConcurrentQueueExtensions
    {
        public static IEnumerable<T> DequeueExisting<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out var item))
                yield return item;
        }

        public static bool TryDequeueAll<T>(this ConcurrentQueue<T> queue, out IList<T> items)
        {
            items = queue.DequeueExisting().ToList();
            return items.Any();
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Monik.Client
{
    public static class QueueClientExtensions
    {
        public static async Task SendBatchAsync(this IQueueClient client,
            IList<Message> messages, long batchSizeLimit)
        {
            foreach (var chunk in messages.ChunkBySize(batchSizeLimit))
            {
                try
                {
                    await client.SendAsync(chunk);
                }
                catch (MessageSizeExceededException)
                {
                    // divide chunk if it can be divided
                    if (chunk.Count > 1)
                    {
                        await client.SendBatchAsync(chunk, batchSizeLimit / 2);
                    }
                    else
                    {
                        // ignore batch which cannot be sent or divided
                    }
                }
            }
        }

        private static IEnumerable<List<Message>> ChunkBySize(this IEnumerable<Message> source, long maxChunkSize)
        {
            var current = (List<Message>) null;
            var sum = 0L;

            foreach (var item in source)
            {
                if (current == null)
                    current = new List<Message> {item};
                else
                    current.Add(item);

                sum += item.Size;
                if (sum < maxChunkSize)
                    continue;

                yield return current;
                current = null;
                sum = 0L;
            }

            if (current != null)
                yield return current;
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Moq;
using NUnit.Framework;

namespace Monik.Client.Azure.Test
{
    [TestFixture]
    public class QueueClientExtensionsTest
    {
        [Test]
        public async Task SendChunked()
        {
            var sentMessages = new List<IList<Message>>();
            var mock = new Mock<IQueueClient>();
            mock.Setup(x => x.SendAsync(It.IsAny<IList<Message>>()))
                .Callback((IList<Message> x) => { sentMessages.Add(x); })
                .Returns(Task.CompletedTask);
            var client = mock.Object;
            var msg1 = new Message {Body = new byte[2000]};
            var msg2 = new Message {Body = new byte[1000]};
            var msg3 = new Message {Body = new byte[500]};
            var msg4 = new Message {Body = new byte[500]};
            var messages = new List<Message> {msg1, msg2, msg3, msg4};

            await client.SendBatchAsync(messages, 1000);

            sentMessages.Should().BeEquivalentTo(new List<IList<Message>>
            {
                new List<Message> {msg1},
                new List<Message> {msg2},
                new List<Message> {msg3, msg4},
            });
        }

        [Test]
        public async Task SendChunked_WithSizeExceededException()
        {
            var sentMessages = new List<IList<Message>>();
            var mock = new Mock<IQueueClient>();
            var client = mock.Object;
            var msg1 = new Message { Body = new byte[2000] };
            var msg2 = new Message { Body = new byte[1000] };
            var msg3 = new Message { Body = new byte[500] };
            var msg4 = new Message { Body = new byte[500] };
            var messages = new List<Message> { msg1, msg2, msg3, msg4 };
            var count = 0;
            mock.Setup(x => x.SendAsync(It.IsAny<IList<Message>>()))
                .Callback((IList<Message> x) =>
                {
                    count++;
                    if (count == 1)
                        throw new MessageSizeExceededException("");

                    sentMessages.Add(x);
                })
                .Returns(Task.CompletedTask);
            
            await client.SendBatchAsync(messages, 3000);

            sentMessages.Should().BeEquivalentTo(new List<IList<Message>>
            {
                new List<Message> {msg1},
                new List<Message> {msg2},
                new List<Message> {msg3, msg4},
            });
        }
    }
}
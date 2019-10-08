using System;
using System.Collections.Generic;
using Monik.Common;

namespace Monik.Service
{
    public class ActiveQueueContext
    {
        public Action<string> OnError;
        public Action<Event> OnReceivedMessage;
        public Action<IEnumerable<Event>> OnReceivedMessages;
    }
}
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Monik.Common;

namespace Monik.Client
{
    public class MonikTestGeneratorInstance : MonikInstance
    {
        private string FSourceName;
        private string FInstanceName;

        private static ConcurrentQueue<Event> FMsgQueue = new ConcurrentQueue<Event>();

        private readonly ManualResetEvent FNewMessageEvent = new ManualResetEvent(false);


        public MonikTestGeneratorInstance(IClientSender aSender, IClientSettings aSettings) : base(aSender, aSettings)
        {
            FSourceName   = aSettings.SourceName;
            FInstanceName = aSettings.InstanceName;
        }

        private Event NewEvent(string instance = null)
        {
            return new Event()
            {
                Created = (long) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds,
                Source  = FSourceName, //Helper.Utf16ToUtf8(FSourceName),
                Instance =
                    string.IsNullOrEmpty(instance) ? FInstanceName : instance //Helper.Utf16ToUtf8(FSourceInstance)
            };
        }

        private void PushLogToSend(string aBody, LevelType aLevel, SeverityType aSeverity, string instance)
        {
            Event msg = NewEvent(instance);

            msg.Lg = new Log()
            {
                Format   = FormatType.Plain,
                Body     = aBody, //Helper.Utf16ToUtf8(_text),
                Level    = aLevel,
                Severity = aSeverity
            };

            FMsgQueue.Enqueue(msg);

            FNewMessageEvent.Set();
        }

        public void LogicInfo(string aBody, string instanceName)
        {
            PushLogToSend(aBody, LevelType.Logic, SeverityType.Info, instanceName);
        }
    }
}
using System;
using Google.Protobuf;
using Monik.Common;
using NUnit.Framework;

namespace Monik.Test
{
    [TestFixture]
    public class ProtoTest
    {
        [Test]
        public void TestEventLog()
        {
            Event _src = new Event
            {
                Created = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Source = "TestSource",
                Instance = "TestInstance",
                Lg = new Log
                {
                    Level = LevelType.Application,
                    Severity = SeverityType.Fatal,
                    Format = FormatType.Json,
                    Body = "TestBody"
                }
            };

            byte[] _buf = _src.ToByteArray();
            Event _dst = Event.Parser.ParseFrom(_buf);

            Assert.AreEqual(_src.Created, _dst.Created);
            Assert.AreEqual(_src.Source, _dst.Source);
            Assert.AreEqual(_src.Instance, _dst.Instance);

            Assert.IsNotNull(_dst.Lg);
            Assert.IsNull(_dst.Ka);

            Assert.AreEqual(_src.Lg.Level, _dst.Lg.Level);
            Assert.AreEqual(_src.Lg.Severity, _dst.Lg.Severity);
            Assert.AreEqual(_src.Lg.Body, _dst.Lg.Body);
        }

        [Test]
        public void TestEventLogEmptyInstnce()
        {
            Event _src = new Event()
            {
                //Created = 0,
                Source = "TestSource",
                //Instance = "",
                Lg = new Log()
                {
                    Level = LevelType.Application,
                    Severity = SeverityType.Fatal,
                    Format = FormatType.Json,
                    Body = "TestBody"
                }
            };

            byte[] _buf = _src.ToByteArray();
            Event _dst = Event.Parser.ParseFrom(_buf);

            Assert.AreEqual(_src.Created, 0);
            Assert.AreEqual(_src.Created, _dst.Created);
            Assert.AreEqual(_src.Source, _dst.Source);
            Assert.AreEqual(_src.Instance, _dst.Instance);
        }
    }//end of class
}

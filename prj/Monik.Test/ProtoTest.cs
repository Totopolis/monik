using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monik.Common;
using Google.Protobuf;

namespace Monik.Test
{
  [TestClass]
  public class ProtoTest
  {
    [TestMethod]
    public void TestEventLog()
    {
      Event _src = new Event()
      {
        Created = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds,
        Source = Helper.Utf16ToUtf8("TestSource"),
        Instance = Helper.Utf16ToUtf8("TestInstance"),
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

      Assert.AreEqual(_src.Created, _dst.Created);
      Assert.AreEqual(_src.Source, _dst.Source);
      Assert.AreEqual(_src.Instance, _dst.Instance);

      Assert.IsNotNull(_dst.Lg);
      Assert.IsNull(_dst.Ka);

      Assert.AreEqual(_src.Lg.Level, _dst.Lg.Level);
      Assert.AreEqual(_src.Lg.Severity, _dst.Lg.Severity);
      Assert.AreEqual(_src.Lg.Body, _dst.Lg.Body);
    }

    [TestMethod]
    public void TestEventLogEmptyInstnce()
    {
      Event _src = new Event()
      {
        //Created = 0,
        Source = Helper.Utf16ToUtf8("TestSource"),
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

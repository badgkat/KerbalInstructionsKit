using KerbalInstructionsKit.Util;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("KerbalInstructionsKit.Tests.TestHelpers.TestLogBootstrap", "KerbalInstructionsKit.Tests")]

namespace KerbalInstructionsKit.Tests.TestHelpers
{
    public sealed class TestLogBootstrap : XunitTestFramework
    {
        public TestLogBootstrap(IMessageSink messageSink) : base(messageSink)
        {
            KikLog.WarnHandler = _ => { };
            KikLog.ErrorHandler = _ => { };
        }
    }
}

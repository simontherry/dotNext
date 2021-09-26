using System.Diagnostics.CodeAnalysis;
using static System.Threading.Timeout;

namespace DotNext.Threading
{
    [ExcludeFromCodeCoverage]
    public sealed class TimeoutTests : Test
    {
        private static void InfiniteTest(Timeout timeout)
        {
            False(timeout.IsExpired);
            True(timeout.IsInfinite);
            if (timeout) throw new Xunit.Sdk.XunitException();
            Equal(InfiniteTimeSpan, timeout);
            Equal(InfiniteTimeSpan, timeout.RemainingTime);
        }

        [Fact]
        public static void DefaultValue()
        {
            InfiniteTest(Timeout.Infinite);
            InfiniteTest(new Timeout(InfiniteTimeSpan));
        }
    }
}
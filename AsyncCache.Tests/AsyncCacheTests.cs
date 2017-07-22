using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AsyncCache.Tests
{
    public class AsyncCacheTests
    {
        int _expensiveTaskCallCount;

        public AsyncCacheTests()
        {
            _expensiveTaskCallCount = 0;
        }

        async Task<string> PerformExpensiveTask(string key, bool withException = false)
        {
            Interlocked.Increment(ref _expensiveTaskCallCount);
            await Task.Delay(500);
            if (withException) throw new ArgumentException();
            return key + " Result";
        }

        [Fact]
        public async Task GivenARequestForAKey_WhenASecondRequestForTheSameKeyIsIssued_ThenTheExpensiveOperationIsNotPerformedAgain()
        {
            var cache = new AsyncCache<string>(TimeSpan.FromMinutes(10));
            var task1 = cache.AddOrGetExisting("Key1", () => PerformExpensiveTask("Key1"));
            await Task.Delay(100);

            var task2 = cache.AddOrGetExisting("Key1", () => PerformExpensiveTask("Key1"));
            var results = await Task.WhenAll(task1, task2);

            Assert.Equal("Key1 Result", results[0]);
            Assert.Equal("Key1 Result", results[1]);
            Assert.Equal(_expensiveTaskCallCount, 1);
        }

        [Fact]
        public async Task GivenARequestForAKey_WhenASecondRequestForADifferentKeyIsIssued_ThenTheExpensiveOperationIsPerformedAgain()
        {
            var cache = new AsyncCache<string>(TimeSpan.FromMinutes(10));
            var task1 = cache.AddOrGetExisting("Key1", () => PerformExpensiveTask("Key1"));
            await Task.Delay(100);

            var task2 = cache.AddOrGetExisting("Key2", () => PerformExpensiveTask("Key2"));
            var results = await Task.WhenAll(task1, task2);

            Assert.Equal("Key1 Result", results[0]);
            Assert.Equal("Key2 Result", results[1]);
            Assert.Equal(_expensiveTaskCallCount, 2);
        }

        [Fact]
        public async Task GivenARequestForAKeyFailed_WhenTheSameKeyIsQueriedAgainSomeTimeLater_ThenTheExpensiveOperationIsPerformedAgain()
        {
            var cache = new AsyncCache<string>(TimeSpan.FromMinutes(10));
            var task1 = Assert.ThrowsAsync<ArgumentException>(() => cache.AddOrGetExisting("Key1", () => PerformExpensiveTask("Key1", true)));
            await Task.Delay(600);

            var task2 = cache.AddOrGetExisting("Key1", () => PerformExpensiveTask("Key1", false));
            await Task.WhenAll(task1, task2);

            Assert.NotNull(task1.Result);
            Assert.Equal("Key1 Result", task2.Result);
            Assert.Equal(_expensiveTaskCallCount, 2);
        }
    }
}

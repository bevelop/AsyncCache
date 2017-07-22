using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AsyncCache.Tests
{
    public class AsyncCacheTests
    {
        int _expensiveTaskCallCount;

        async Task<string> PerformExpensiveTask(string key)
        {
            Interlocked.Increment(ref _expensiveTaskCallCount);
            await Task.Delay(500);
            return key + " Result";
        }

        [Fact]
        public async Task Test1()
        {
            var cache = new AsyncCache<string>(TimeSpan.FromSeconds(2));
            var task1 = cache.AddOrGetExisting("Key1", () => PerformExpensiveTask("Key1"));
            var task2 = cache.AddOrGetExisting("Key2", () => PerformExpensiveTask("Key2"));
            var task3 = cache.AddOrGetExisting("Key1", () => PerformExpensiveTask("Key1"));

            var results = await Task.WhenAll(task1, task2, task3);

            Assert.Equal("Key1 Result", results[0]);
            Assert.Equal("Key2 Result", results[1]);
            Assert.Equal("Key1 Result", results[2]);
            Assert.Equal(_expensiveTaskCallCount, 2);
        }
    }
}

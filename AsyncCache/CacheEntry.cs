using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCache
{
    class CacheEntry<T>
    {
        readonly SemaphoreSlim _semaphore;
        readonly Func<Task<T>> _task;
        bool _hasResult;
        T _result;

        public CacheEntry(Func<Task<T>> task)
        {
            _task = task;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<T> Get()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!_hasResult)
                {
                    _result = await _task();
                    _hasResult = true;
                }

                return _result;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
# AsyncCache

This simple cache implementation accomplishes 2 goals:

1. Allows a cached data to be accessed asynchronously using an async `AddOrGetExisting()` method that accepts an `Task` factory.
2. If multiple requests for the same key are received simultaneously, the expensive task is only performed once, while the remaining requests asynchronously wait for its result.

This solution also avoids caching of exceptions.

See tests for usage example.

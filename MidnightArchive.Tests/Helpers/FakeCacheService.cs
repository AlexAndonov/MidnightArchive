using MidnightArchive.Core.Contracts;

namespace MidnightArchive.Tests.Helpers
{
	public class FakeCacheService : ICacheService
	{
		private readonly Dictionary<string, object> cache = new();

		public Task<T?> GetAsync<T>(string key)
		{
			if (!cache.TryGetValue(key, out object? value))
			{
				return Task.FromResult(default(T));
			}

			return Task.FromResult((T?)value);
		}

		public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
		{
			cache[key] = value!;
			return Task.CompletedTask;
		}

		public Task RemoveAsync(string key)
		{
			cache.Remove(key);
			return Task.CompletedTask;
		}
	}
}
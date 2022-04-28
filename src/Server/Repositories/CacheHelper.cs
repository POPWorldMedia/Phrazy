using Microsoft.Extensions.Caching.Memory;

namespace Phrazy.Server.Repositories;

public interface ICacheHelper
{
	void SetCacheObject(string key, object value, double seconds);
	void SetCacheObject(string key, object value, DateTime date);
	void RemoveCacheObject(string key);
	T GetCacheObject<T>(string key);
}

public class CacheHelper : ICacheHelper
{
	public CacheHelper()
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (_cache != null) return;
		var options = new MemoryCacheOptions();
		_cache = new MemoryCache(options);
	}
	
	private static IMemoryCache _cache = null!;
	
	public void SetCacheObject(string key, object value, double seconds)
	{
		var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(seconds) };
		_cache.Set(key, value, options);
	}

	public void SetCacheObject(string key, object value, DateTime date)
	{
		var options = new MemoryCacheEntryOptions {AbsoluteExpiration = date};
		_cache.Set(key, value, options);
	}

	public void RemoveCacheObject(string key)
	{
		_cache.Remove(key);
	}

	public T GetCacheObject<T>(string key)
	{
		var success = _cache.TryGetValue(key, out var cacheObject);
		if (success)
			return (T)cacheObject;
		return default!;
	}
}
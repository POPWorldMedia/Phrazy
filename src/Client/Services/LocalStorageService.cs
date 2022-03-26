using System.Text.Json;
using Microsoft.JSInterop;

namespace Phrazy.Client.Services;

public interface ILocalStorageService
{
	Task Set(string key, object item);
	Task<T?> Get<T>(string key);
}

public class LocalStorageService : ILocalStorageService
{
	private readonly IJSRuntime _jsRuntime;

	public LocalStorageService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public async Task Set(string key, object item)
	{
		var serializedString = JsonSerializer.Serialize(item);
		await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, serializedString);
	}

	public async Task<T?> Get<T>(string key)
	{
		var serializedString = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
		if (serializedString == null)
			return default;
		var item = JsonSerializer.Deserialize<T>(serializedString);
		return item;
	}
}
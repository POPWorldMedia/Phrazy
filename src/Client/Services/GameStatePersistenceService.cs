using Microsoft.JSInterop;
using Phrazy.Client.Models;
using System.Text.Json;

namespace Phrazy.Client.Services;

public interface IGameStatePersistenceService
{
	Task Save(GameState gameState);
	Task<GameState?> Load();
}

public class GameStatePersistenceService : IGameStatePersistenceService
{
	private readonly IJSRuntime _jsRuntime;
	private const string StorageKey = "currentGameState";

	public GameStatePersistenceService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public async Task Save(GameState gameState)
	{
		var serializedString = JsonSerializer.Serialize(gameState);
		await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, serializedString);
	}

	public async Task<GameState?> Load()
	{
		var serializedString = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
		if (serializedString == null)
			return null;
		var item = JsonSerializer.Deserialize<GameState>(serializedString);
		return item;
	}
}
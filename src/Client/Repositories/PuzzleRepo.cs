using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Phrazy.Shared;
using Phrazy.Shared.Models;

namespace Phrazy.Client.Repositories;

public interface IPuzzleRepo
{
	Task<PuzzlePayload> GetPuzzleWithIdentifier(string id, long ticks);
	Task PutResults(ResultPayload resultPayload);
	Task<LastResultPayload?> GetLastResultWithIdentifier(string id);
}

public class PuzzleRepo : IPuzzleRepo
{
	private readonly HttpClient _httpClient;

	public PuzzleRepo(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<PuzzlePayload> GetPuzzleWithIdentifier(string id, long ticks)
	{
		var puzzlePayload = await _httpClient.GetFromJsonAsync<PuzzlePayload>(ApiPaths.Puzzle.GetWithIdentifier.Replace("{id}", id) + $"?ticks={ticks}");
		return puzzlePayload!;
	}

	public async Task PutResults(ResultPayload resultPayload)
	{
		await _httpClient.PutAsJsonAsync(ApiPaths.Puzzle.PutResult, resultPayload);
	}

	public async Task<LastResultPayload?> GetLastResultWithIdentifier(string id)
	{
		var result = await _httpClient.GetAsync(ApiPaths.Puzzle.GetLastResult.Replace("{id}", id));
		if (result.StatusCode != HttpStatusCode.OK)
			return null;
		var payload = await result.Content.ReadAsStringAsync();
		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var lastResultPayload = JsonSerializer.Deserialize<LastResultPayload>(payload, options);
		return lastResultPayload!;
	}
}
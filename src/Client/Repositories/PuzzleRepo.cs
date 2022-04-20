using System.Net.Http.Json;
using Phrazy.Shared;
using Phrazy.Shared.Models;

namespace Phrazy.Client.Repositories;

public interface IPuzzleRepo
{
	Task<PuzzlePayload> GetPuzzleWithIdentifier(string id);
	Task PutResults(ResultPayload resultPayload);
	Task<LastResultPayload> GetLastResultWithIdentifier(string id);
}

public class PuzzleRepo : IPuzzleRepo
{
	private readonly HttpClient _httpClient;

	public PuzzleRepo(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<PuzzlePayload> GetPuzzleWithIdentifier(string id)
	{
		var puzzlePayload = await _httpClient.GetFromJsonAsync<PuzzlePayload>(ApiPaths.Puzzle.GetWithIdentifier.Replace("{id}", id));
		return puzzlePayload!;
	}

	public async Task PutResults(ResultPayload resultPayload)
	{
		await _httpClient.PutAsJsonAsync(ApiPaths.Puzzle.PutResult, resultPayload);
	}

	public async Task<LastResultPayload> GetLastResultWithIdentifier(string id)
	{
		var lastResultPayload = await _httpClient.GetFromJsonAsync<LastResultPayload>(ApiPaths.Puzzle.GetLastResult.Replace("{id}", id));
		return lastResultPayload!;
	}
}
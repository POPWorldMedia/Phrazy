using Phrazy.Client.Models;
using Phrazy.Client.Repositories;
using Phrazy.Shared.Models;

namespace Phrazy.Client.Services;

public interface IPuzzleService
{
	Task<PuzzleDefinition> GetCurrentPuzzle();
	void SendResults(string deviceID, string hash, string puzzleID, Results results);
	Task<LastResultPayload?> GetLastResult();
}

public class PuzzleService : IPuzzleService
{
	private readonly IPuzzleRepo _puzzleRepo;
	private readonly IDeviceIDService _deviceIDService;

	public PuzzleService(IPuzzleRepo puzzleRepo, IDeviceIDService deviceIDService)
	{
		_puzzleRepo = puzzleRepo;
		_deviceIDService = deviceIDService;
	}

	public async Task<PuzzleDefinition> GetCurrentPuzzle()
	{
		var identifier = await _deviceIDService.GetDeviceID();
		var puzzlePayload = await _puzzleRepo.GetPuzzleWithIdentifier(identifier, DateTime.Now.Date.Ticks);
		if (string.IsNullOrEmpty(puzzlePayload.Phrase))
			return new PuzzleDefinition();
		var base64EncodedBytes = Convert.FromBase64String(puzzlePayload.Phrase);
		var puzzle = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		var definition = new PuzzleDefinition
		{
			Phrase = puzzle,
			PuzzleID = puzzlePayload.PuzzleID,
			Hash = puzzlePayload.Hash
		};
		return definition;
	}

	public async void SendResults(string deviceID, string hash, string puzzleID, Results results)
	{
		var resultPayload = new ResultPayload
		{
			DeviceID = deviceID,
			Hash = hash,
			PuzzleID = puzzleID,
			Results = results

		};
		await _puzzleRepo.PutResults(resultPayload);
	}

	public async Task<LastResultPayload?> GetLastResult()
	{
		var identifier = await _deviceIDService.GetDeviceID();
		var lastResult = await _puzzleRepo.GetLastResultWithIdentifier(identifier);
		return lastResult;
	}
}
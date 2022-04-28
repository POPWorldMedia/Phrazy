using System.Security.Cryptography;
using Phrazy.Shared.Models;
using System.Text;
using System.Text.Json;
using Phrazy.Server.Repositories;

namespace Phrazy.Server.Services;

public interface IPuzzleService
{
	Task<PuzzlePayload> GetPayloadForToday(string identifier, DateTime date);
	Task<bool> SaveResult(ResultPayload resultPayload);
	Task<LastResultPayload> GetLastResultByDeviceID(string deviceID);
}

public class PuzzleService : IPuzzleService
{
	private readonly IResultRepository _resultRepository;
	private readonly IPuzzleRepository _puzzleRepository;

	public PuzzleService(IResultRepository resultRepository, IPuzzleRepository puzzleRepository)
	{
		_resultRepository = resultRepository;
		_puzzleRepository = puzzleRepository;
	}

	public async Task<PuzzlePayload> GetPayloadForToday(string identifier, DateTime date)
	{
		var record = await _puzzleRepository.GetPuzzleByDate(date);
		if (record == null)
			return new PuzzlePayload();
		var payload = new PuzzlePayload();
		var unencodedPuzzle = record.Phrase;
		var encodedPuzzle = EncodeString(unencodedPuzzle);
		payload.Phrase = encodedPuzzle;
		var hash = GetHash(payload.PuzzleID, identifier);
		payload.Hash = hash;
		payload.PuzzleID = record.PuzzleID;
		payload.PlayDate = record.PlayDate;
		return payload;
	}

	private string EncodeString(string text)
	{
		var plainTextBytes = Encoding.UTF8.GetBytes(text);
		return Convert.ToBase64String(plainTextBytes);
	}

	private string GetHash(string puzzleID, string deviceID)
	{
		var combined = puzzleID + "batshitphrazy" + deviceID;
		var input = Encoding.UTF8.GetBytes(combined);
		using var sha256 = SHA256.Create();
		var output = sha256.ComputeHash(input);
		return Convert.ToBase64String(output);
	}

	public async Task<bool> SaveResult(ResultPayload resultPayload)
	{
		var hash = GetHash(resultPayload.PuzzleID, resultPayload.DeviceID);
		if (hash == resultPayload.Hash)
		{
			var resultString = JsonSerializer.Serialize(resultPayload.Results);
			await _resultRepository.SaveResult(resultPayload.DeviceID, resultPayload.PuzzleID, resultPayload.Results.Score, resultPayload.Results.Seconds, null, DateTime.UtcNow, resultPayload.Results.IsWin, resultString);
			return true;
		}

		return false;
	}

	public async Task<LastResultPayload> GetLastResultByDeviceID(string deviceID)
	{
		return await _resultRepository.GetLastCalculatedResultByDeviceID(deviceID);
	}
}
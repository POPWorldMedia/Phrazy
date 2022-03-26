using System.Security.Cryptography;
using Phrazy.Shared.Models;
using System.Text;
using System.Text.Json;
using Phrazy.Server.Repositories;

namespace Phrazy.Server.Services;

public interface IPuzzleService
{
	Task<PuzzlePayload> GetPayloadForToday(string identifier);
	Task<bool> SaveResult(ResultPayload resultPayload);
}

public class PuzzleService : IPuzzleService
{
	private readonly IResultRepository _resultRepository;

	public PuzzleService(IResultRepository resultRepository)
	{
		_resultRepository = resultRepository;
	}

	public async Task<PuzzlePayload> GetPayloadForToday(string identifier)
	{
		string[] phrases =
		{
			"the bigger they are, the harder they fall",
			"this aggression will not stand",
			"i am not throwing away my shot",
			"the red coats are coming",
			"don't judge a book by its cover",
			"teach an old dog new tricks",
			"the best things in life are free",
			"it's the most wonderful time of the year",
			"walk a mile in her shoes",
			"you can't always get what you want",
			"here's looking at you, kid",
			"i do not like green eggs and ham",
			"april showers bring may flowers",
			"hey man, there's a beverage here!",
			"we don't talk about bruno",
			"when you wish upon a star",
			"i'm only happy when it rains"
		};
		var index = Random.Shared.Next(0, 17);
		var unencodedPuzzle = phrases[index];
		var encodedPuzzle = EncodeString(unencodedPuzzle);
		var payload = new PuzzlePayload();
		payload.Puzzle = encodedPuzzle;
		payload.PuzzleID = unencodedPuzzle;
		payload.Date = DateTime.UtcNow.Date;
		var hash = GetHash(payload.PuzzleID, identifier);
		payload.Hash = hash;
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
			await _resultRepository.SaveResult(resultPayload.DeviceID, resultPayload.PuzzleID, resultPayload.Results.Score, null, DateTime.UtcNow, resultPayload.Results.IsWin, resultString);
			return true;
		}

		return false;
	}
}
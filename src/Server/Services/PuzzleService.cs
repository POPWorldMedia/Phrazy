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
			"too cool for school",
			"immigrants, we get the job done",
			"it takes one to know one",
			"don't get bent out of shape",
			"get in loser we're going shopping",
			"don't put all your eggs in one basket",
			"there's no crying in baseball!",
			"may the force be with you, always",
			"there's no place like home",
			"the bend and snap, works every time",
			"houston we have a problem",
			"what's love got to do with it",
			"smells like teen spirit",
			"the hitchhiker's guide to the galaxy",
			"shake it like a polaroid picture",
			"the girl with the dragon tattoo",
			"i want to hold your hand",
			"this is not my beautiful wife",
			"give me something to believe in",
			"the hunt for red october",
			"the best things in life are free",
			"out of the frying pan and into the fire",
			"when it rains it pours",
			"between a rock and a hard place",
			"we don't talk about bruno",
			"curiosity killed the cat",
			"don't cry over spilled milk",
			"back to the drawing board",
			"the pirates don't eat the tourists",
			"if at first you don't succeed"
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
			await _resultRepository.SaveResult(resultPayload.DeviceID, resultPayload.PuzzleID, resultPayload.Results.Score, resultPayload.Results.Seconds, null, DateTime.UtcNow, resultPayload.Results.IsWin, resultString);
			return true;
		}

		return false;
	}
}
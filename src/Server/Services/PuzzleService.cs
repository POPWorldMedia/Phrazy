using Phrazy.Shared.Models;

namespace Phrazy.Server.Services;

public interface IPuzzleService
{
	Task<PuzzlePayload> GetPayloadForToday(string identifier);
}

public class PuzzleService : IPuzzleService
{
	public PuzzleService()
	{
	}

	public async Task<PuzzlePayload> GetPayloadForToday(string identifier)
	{
		string[] phrases =
		{
			"the bigger they are, the harder they fall",
			"this aggression will not stand",
			"i am not throwing away my shot",
			"the red coats are coming",
			"i'm addicted to you, don't you know that you're toxic?",
			"do not judge a book by its cover",
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
		payload.ID = "123abc";
		payload.Date = DateTime.UtcNow.Date;
		payload.Hash = "fhweufhe";
		payload.Puzzle = encodedPuzzle;
		return payload;
	}

	private string EncodeString(string text)
	{
		var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(text);
		return System.Convert.ToBase64String(plainTextBytes);
	}
}
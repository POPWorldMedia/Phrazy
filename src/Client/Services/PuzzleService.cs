using Phrazy.Client.Models;
using Phrazy.Client.Repositories;

namespace Phrazy.Client.Services;

public interface IPuzzleService
{
	Task<PuzzleDefinition> GetCurrentPuzzle();
}

public class PuzzleService : IPuzzleService
{
	private readonly IPuzzleRepo _puzzleRepo;

	public PuzzleService(IPuzzleRepo puzzleRepo)
	{
		_puzzleRepo = puzzleRepo;
	}

	public async Task<PuzzleDefinition> GetCurrentPuzzle()
	{
		var identifier = "1";
		var puzzlePayload = await _puzzleRepo.GetPuzzleWithIdentifier(identifier);
		var base64EncodedBytes = System.Convert.FromBase64String(puzzlePayload.Puzzle);
		var puzzle = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		var definition = new PuzzleDefinition
		{
			Puzzle = puzzle
		};
		return definition;
	}
}